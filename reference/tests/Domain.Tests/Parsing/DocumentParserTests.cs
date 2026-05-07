using FluentAssertions;
using Moq;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using DomainFile = Mnemi.Domain.Entities.File;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class DocumentParserTests
{
    [Fact]
    public void DocumentParser_parses_top_level_document_tags()
    {
        var firstGroup = new Group(new[] { "Spanish" });
        var secondGroup = new Group(new[] { "Spanish", "Verbs" });
        var prunedGroups = new[] { secondGroup };

        var groupParserMock = new Mock<IGroupParser>();
        groupParserMock
            .SetupSequence(parser => parser.Parse(It.IsAny<string>()))
            .Returns(firstGroup)
            .Returns(secondGroup);
        groupParserMock
            .Setup(parser => parser.PruneAncestors(It.IsAny<IEnumerable<Group>>()))
            .Returns(prunedGroups);

        var file = new DomainFile(
            "deck.md",
            "notes/deck.md",
            null,
            null,
            """
#spanish
#spanish/verbs

What is "hablar"?::to speak
""");

    var documentParser = new DocumentParser(groupParserMock.Object);
        var document = documentParser.Parse(file);

    document.DocumentTags.Should().ContainSingle();
    document.DocumentTags[0].DisplayPath.Should().Be("Spanish::Verbs");
    groupParserMock.Verify(parser => parser.Parse("spanish"), Times.Once);
    groupParserMock.Verify(parser => parser.Parse("spanish/verbs"), Times.Once);
    groupParserMock.Verify(parser => parser.PruneAncestors(It.IsAny<IEnumerable<Group>>()), Times.Once);
    }
}