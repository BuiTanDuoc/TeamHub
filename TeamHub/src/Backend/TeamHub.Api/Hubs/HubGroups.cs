namespace TeamHub.Api.Hubs;

/// <summary>Quy ước tên group của SignalR.</summary>
public static class HubGroups
{
    public static string User(Guid userId) => $"user-{userId}";
    public static string Team(Guid teamId) => $"team-{teamId}";
    public static string Project(Guid projectId) => $"project-{projectId}";
}
