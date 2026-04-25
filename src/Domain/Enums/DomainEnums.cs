namespace Mnemi.Domain.Enums;

public enum DeckStatus
{
    Active,
    Completed,
    Archived
}

public enum CardState
{
    New,
    Learning,
    Review,
    Learned
}

public enum ReviewRating
{
    Again = 0,
    Hard = 1,
    Good = 2,
    Easy = 3
}

public enum ViewState
{
    Phone = 0,
    Tablet = 1,
    Desktop = 2
}
