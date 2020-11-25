using Compiler.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

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
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);
            SyntaxToken token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            return kinds.Select(kind => new object[] { kind });
        }
    }
}