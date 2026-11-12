namespace TeamHub.Infrastructure.Identity;

public sealed class SeedOptions
{
    public const string SectionName = "Seed:Admin";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = "System Admin";
}
