using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Users;
using TeamHub.Infrastructure.Persistence;

namespace TeamHub.Infrastructure.Identity;

public sealed class UserAdministration(UserManager<AppUser> users, AppDbContext db, TimeProvider timeProvider)
    : IUserAdministration
{
    public async Task<AccountOperationResult> CreateUserAsync(
        string email, string fullName, string password, bool isSystemAdmin, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        if (await users.FindByEmailAsync(normalizedEmail) is not null)
            return AccountOperationResult.Fail("Email đã được sử dụng.");

        var user = new AppUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true, // tài khoản do quản trị viên tạo nên coi như đã xác minh
            FullName = fullName.Trim(),
            IsSystemAdmin = isSystemAdmin,
            CreatedAt = timeProvider.GetUtcNow()
        };

        var result = await users.CreateAsync(user, password);
        return result.Succeeded
            ? AccountOperationResult.Ok(user.Id)
            : AccountOperationResult.Fail(result.Errors.Select(e => e.Description));
    }

    public async Task<AccountOperationResult> ResetPasswordAsync(
        Guid userId, string newPassword, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null)
            return AccountOperationResult.Fail("Không tìm thấy người dùng.");

        var result = await ApplyPasswordAsync(user, newPassword);
        if (result.Succeeded)
            await RevokeSessionsAsync(userId, cancellationToken);

        return result;
    }

    public async Task<AccountOperationResult> ChangePasswordAsync(
        Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null)
            return AccountOperationResult.Fail("Không tìm thấy người dùng.");

        if (!await users.HasPasswordAsync(user))
            return AccountOperationResult.Fail("Tài khoản đăng nhập bằng Google nên chưa có mật khẩu để đổi.");

        if (!await users.CheckPasswordAsync(user, currentPassword))
            return AccountOperationResult.Fail("Mật khẩu hiện tại không đúng.");

        var result = await ApplyPasswordAsync(user, newPassword);
        if (result.Succeeded)
            await RevokeSessionsAsync(userId, cancellationToken);

        return result;
    }

    public async Task RevokeSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTimeOffset?)now), cancellationToken);
    }

    /// <summary>
    /// Kiểm tra mật khẩu mới theo chính sách (Identity:Password) rồi ghi băm mới trong một lần cập nhật,
    /// để mật khẩu không bị mất nếu mật khẩu mới không hợp lệ.
    /// </summary>
    private async Task<AccountOperationResult> ApplyPasswordAsync(AppUser user, string newPassword)
    {
        var errors = new List<string>();
        foreach (var validator in users.PasswordValidators)
        {
            var validation = await validator.ValidateAsync(users, user, newPassword);
            if (!validation.Succeeded)
                errors.AddRange(validation.Errors.Select(e => e.Description));
        }

        if (errors.Count > 0)
            return AccountOperationResult.Fail(errors);

        user.PasswordHash = users.PasswordHasher.HashPassword(user, newPassword);
        var update = await users.UpdateAsync(user);
        if (!update.Succeeded)
            return AccountOperationResult.Fail(update.Errors.Select(e => e.Description));

        await users.ResetAccessFailedCountAsync(user); // gỡ trạng thái khóa do đăng nhập sai trước đó
        return AccountOperationResult.Ok();
    }
}
