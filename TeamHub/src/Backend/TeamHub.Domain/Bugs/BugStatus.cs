namespace TeamHub.Domain.Bugs;

public enum BugStatus
{
    New = 0,
    Confirmed = 1,
    Assigned = 2,
    InProgress = 3,
    Fixed = 4,
    ReadyToRetest = 5,
    Verified = 6,
    Closed = 7,
    Reopened = 8,

    // Nhánh kết thúc không qua fix
    WontFix = 9,
    Duplicate = 10,
    CannotReproduce = 11
}
