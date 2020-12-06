using System;
using System.Collections.Generic;
using System.Linq;

using Compiler.CodeAnalysis.Syntax;

using Xunit;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Cover_AllTokens( )
        {
            SortedSet<SyntaxKind> untestedTokensKinds =
                new SortedSet<SyntaxKind>(Enum.GetValues(typeof(SyntaxKind))
                                              .Cast<SyntaxKind>()
                                              .Where(k => k.ToString().EndsWith("Keyword") || k.ToString().EndsWith("Token")));
            untestedTokensKinds.Remove(SyntaxKind.EndOfFileToken);
            untestedTokensKinds.Remove(SyntaxKind.InvalidToken);
            untestedTokensKinds.ExceptWith(GetTokens().Select(t => t.kind));
            Assert.Empty(untestedTokensKinds);
        }

        [Theory]
        [MemberData(nameof(GetTokenData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
        {
            SyntaxToken token = Assert.Single(SyntaxTree.ParseTokens(text));
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairData))]
        public void Lexer_Lexes_Token_Pairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
        {
            SyntaxToken[] tokens = SyntaxTree.ParseTokens(t1Text + t2Text).ToArray();
            Assert.Equal(2, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, t2Kind);
            Assert.Equal(tokens[1].Text, t2Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairWithSeparatorData))]
        public void Lexer_Lexes_Token_Pairs_With_Separator(SyntaxKind t1Kind, string t1Text, SyntaxKind spKind, string spText, SyntaxKind t2Kind, string t2Text)
        {
            SyntaxToken[] tokens = SyntaxTree.ParseTokens(t1Text + spText + t2Text).ToArray();
            Assert.Equal(3, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, spKind);
            Assert.Equal(tokens[1].Text, spText);
            Assert.Equal(tokens[2].Kind, t2Kind);
            Assert.Equal(tokens[2].Text, t2Text);
        }

        public static IEnumerable<object[]> GetTokenData( )
            => GetTokens().Select(t => new object[] { t.kind, t.text });

        public static IEnumerable<object[]> GetTokenPairData( )
            => GetTokenPairs().Select(t => new object[] { t.t1Kind, t.t1Text, t.t2Kind, t.t2Text });

        public static IEnumerable<object[]> GetTokenPairWithSeparatorData( ) => GetTokenPairsWithSeparator().Select(t => new object[] {
                                                                                                                   t.t1Kind, t.t1Text,
                                                                                                                   t.spkind, t.spText,
                                                                                                                   t.t2Kind, t.t2Text });

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens( )
            => Enum.GetValues(typeof(SyntaxKind))
                   .Cast<SyntaxKind>()
                   .Select(k => (kind: k, text: SyntaxFacts.GetText(k)))
                   .Where(t => t.text != null)
                   .Concat(GetSeparators())
                   .Concat(new[] {
                        (SyntaxKind.NumberToken, "1"),
                        (SyntaxKind.NumberToken, "123456"),
                        (SyntaxKind.IdentifierToken, "a"),
                        (SyntaxKind.IdentifierToken, "abc"),
                        (SyntaxKind.IdentifierToken, "testing"),
                        (SyntaxKind.StringToken, "\"test\""),
                        (SyntaxKind.StringToken, "\"te\\\"st\"")
                   });

        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators( ) => new[]
        {
            (SyntaxKind.WhiteSpaceToken, " "),
            (SyntaxKind.WhiteSpaceToken, "  "),
            (SyntaxKind.WhiteSpaceToken, "\r"),
            (SyntaxKind.WhiteSpaceToken, "\n"),
            (SyntaxKind.WhiteSpaceToken, "\r\n"),
        };

        private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
            => ((t1Kind == SyntaxKind.IdentifierToken || t1Kind.ToString().EndsWith("Keyword")) &&
               (t2Kind == SyntaxKind.IdentifierToken || t2Kind.ToString().EndsWith("Keyword"))) ||
               ((t1Kind == SyntaxKind.ExclamationToken || t1Kind == SyntaxKind.AssigmentToken ||
               t1Kind == SyntaxKind.LessThenToken || t1Kind == SyntaxKind.GreaterThenToken) &&
               (t2Kind == SyntaxKind.AssigmentToken || t2Kind == SyntaxKind.DoubleEqualsToken)) ||
               (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken) ||
               (t1Kind == SyntaxKind.PipeToken && (t2Kind == SyntaxKind.PipePipeToken ||
               t2Kind == SyntaxKind.PipeToken)) || (t1Kind == SyntaxKind.AmpersandToken &&
               (t2Kind == SyntaxKind.AmpersandAmpersandToken || t2Kind == SyntaxKind.AmpersandToken));

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs( )
        {
            foreach ((SyntaxKind kind1, string text1) in GetTokens())
                foreach ((SyntaxKind kind2, string text2) in GetTokens())
                {
                    if (kind1 == SyntaxKind.WhiteSpaceToken && kind2 == SyntaxKind.WhiteSpaceToken)
                        continue;
                    if (!RequiresSeparator(kind1, kind2))
                        yield return (kind1, text1, kind2, text2);
                }
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind spkind, string spText, SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator( )
        {
            foreach ((SyntaxKind kind1, string text1) in GetTokens())
                foreach ((SyntaxKind kind2, string text2) in GetTokens())
                {
                    if (kind1 == SyntaxKind.WhiteSpaceToken && kind2 == SyntaxKind.WhiteSpaceToken)
                        continue;
                    if (RequiresSeparator(kind1, kind2))
                        foreach ((SyntaxKind kind, string text) in GetSeparators())
                            yield return (kind1, text1, kind, text, kind2, text2);
                }
        }
    }
}