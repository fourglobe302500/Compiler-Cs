using Compiler.CodeAnalysis.Text;
using Xunit;

namespace Compiler.Tests.CodeAnalysis.Text
{
    public class SourceTextTest
    {
        [Theory]
        [InlineData(".", 1)]
        [InlineData(".\r\n", 2)]
        [InlineData(".\r\n\r\n", 3)]
        public void SourceText_IncludesLastLine(string text, int numberOfLines)
        {
            SourceText sourceText = SourceText.From(text);
            Assert.Equal(sourceText.Lines.Length, numberOfLines);
        }
    }
}