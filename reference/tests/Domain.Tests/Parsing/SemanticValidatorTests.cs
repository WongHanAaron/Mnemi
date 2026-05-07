using FluentAssertions;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Parsing;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class SemanticValidatorTests
{
    [Fact]
    public void SemanticValidator_reports_mcq_without_correct_answer()
    {
        var validator = new SemanticValidator(new CardParserUtilities());
        var bound = new BoundDocument(
            new[]
            {
                new BoundQuestion(
                    new InlineCardSyntaxNode(1, "What is 2+2?::*3|*5"),
                    new MetadataResult(LearningState.New, Array.Empty<Group>()))
            },
            Array.Empty<Group>());

        var diagnostics = validator.Validate(bound);

        diagnostics.HasErrors.Should().BeTrue();
        diagnostics.Diagnostics.Should().Contain(diagnostic => diagnostic.Code == "DSL003");
    }
}
