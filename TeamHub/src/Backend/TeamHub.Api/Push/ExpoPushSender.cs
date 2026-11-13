using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TeamHub.Application.Notifications;

namespace TeamHub.Api.Push;

/// <summary>Gửi push qua Expo Push API (mỗi lần tối đa 100 tin) và trả về các token không còn hợp lệ.</summary>
public sealed class ExpoPushSender(HttpClient http, IOptions<ExpoPushOptions> options, ILogger<ExpoPushSender> logger)
    : IPushSender
{
    // Giới hạn của Expo cho mỗi request
    private const int MaxMessagesPerRequest = 100;

    private sealed record ExpoResponse(List<ExpoTicket>? Data);
    private sealed record ExpoTicket(string? Status, string? Message, ExpoTicketDetails? Details);
    private sealed record ExpoTicketDetails(string? Error);

    public async Task<IReadOnlyList<string>> SendAsync(IReadOnlyList<PushMessage> messages, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var invalidTokens = new List<string>();

        foreach (var chunk in messages.Chunk(MaxMessagesPerRequest))
        {
            var payload = chunk.Select(m => new { to = m.To, title = m.Title, body = m.Body, data = m.Data, sound = "default" });

            using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(settings.AccessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);

            using var response = await http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ExpoResponse>(cancellationToken);

            // Kết quả trả về cùng thứ tự với tin đã gửi
            var tickets = result?.Data ?? [];
            for (var i = 0; i < Math.Min(tickets.Count, chunk.Length); i++)
            {
                if (tickets[i].Status != "error")
                    continue;

                if (tickets[i].Details?.Error == "DeviceNotRegistered")
                    invalidTokens.Add(chunk[i].To);
                else
                    logger.LogWarning("Expo từ chối một tin push: {Message}", tickets[i].Message);
            }
        }

        return invalidTokens;
    }
}
