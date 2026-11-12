namespace TeamHub.Application.Common.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// True với SystemAdmin và với tiến trình nền (Hangfire job) để bỏ qua lọc theo team.
    /// </summary>
    bool IsSystemAdmin { get; }
}
