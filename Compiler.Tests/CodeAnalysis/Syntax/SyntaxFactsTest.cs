using System;
using System.Collections.Generic;
using System.Linq;

using Compiler.CodeAnalysis.Syntax;

using Xunit;
namespace Compiler.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactsTest
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxFact_GetText_BoundTrips(SyntaxKind kind)
        {
            string text = SyntaxFacts.GetText(kind);
            if (text == null)
                return;
            SyntaxToken token = Assert.Single(SyntaxTree.ParseTokens(text));
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }
        public static IEnumerable<object[]> GetSyntaxKindData( ) => ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind))).Select(kind => (new object[] { kind }));
    }
}