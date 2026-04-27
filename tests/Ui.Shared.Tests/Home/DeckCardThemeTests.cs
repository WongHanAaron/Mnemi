using Bunit;
using Mnemi.Ui.Shared.Components.Home;
using Mnemi.Ui.Shared.Models.Home;
using Xunit;

namespace Mnemi.Ui.Shared.Tests.Home;

public sealed class DeckCardThemeTests : TestContext
{
    [Fact]
    public void DeckCard_RendersAudioCardStructureAndActionSelector()
    {
        var model = new StudyDeckSummaryViewModel(
            DeckId: "audio-deck",
            Title: "Audio Deck",
            Subtitle: "Playlist style",
            ProgressPercent: 80,
            StatusLabel: "Strong",
            ArtworkToken: "AD",
            DueCount: 2,
            IsPinned: true,
            PrimaryAction: new DeckPrimaryAction("play", "Play", "audio-deck"),
            State: SectionDataState.Populated);

        var cut = RenderComponent<DeckCard>(parameters => parameters
            .Add(component => component.Model, model));

        Assert.NotNull(cut.Find("[data-testid='deck-card-audiodeck']"));
        Assert.NotNull(cut.Find("[data-testid='deck-card-action-audiodeck']"));
        Assert.NotNull(cut.Find(".home-deck-card__cover"));
        Assert.NotNull(cut.Find(".home-deck-card__meta"));
    }
}
