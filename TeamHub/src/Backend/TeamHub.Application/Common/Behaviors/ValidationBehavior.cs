using System.Text.Json;
using FluentValidation;
using Mediator;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Common.Behaviors;

/// <summary>Chạy mọi validator của message; handler không tự validate lại.</summary>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TMessage>(message);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
        {
            // Tên field theo camelCase để khớp JSON của client
            var errors = failures
                .GroupBy(f => JsonNamingPolicy.CamelCase.ConvertName(f.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            throw new RequestValidationException(errors);
        }

        return await next(message, cancellationToken);
    }
}
