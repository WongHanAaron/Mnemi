using FluentAssertions;
using Mnemi.Domain.Parsing;
using Xunit;

namespace Mnemi.Domain.Tests.Parsing;

public class SourceReaderTests
{
    [Fact]
    public void SourceReader_reads_lines_with_line_numbers_and_trimmed_text()
    {
        var reader = new SourceReader();

        var source = reader.Read("""
 line-one 

line-two
""");

        source.Lines.Should().HaveCount(3);
        source.Lines[0].LineNumber.Should().Be(1);
        source.Lines[0].TrimmedText.Should().Be("line-one");
        source.Lines[1].TrimmedText.Should().BeEmpty();
        source.Lines[2].LineNumber.Should().Be(3);
        source.Lines[2].TrimmedText.Should().Be("line-two");
    }
}
