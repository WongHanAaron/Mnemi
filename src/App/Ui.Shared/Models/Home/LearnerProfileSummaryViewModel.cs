namespace Ui.Shared.Models.Home;

public sealed record LearnerProfileSummaryViewModel(
    string? DisplayName,
    string GreetingFallback,
    string? AvatarInitials);
