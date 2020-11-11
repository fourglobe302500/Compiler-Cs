using System.Linq;
using Xunit;
using Compiler.CodeAnalysis.Syntax;
using System.Collections.Generic;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    public class LexerTest
    {
        [Theory]
        [MemberData(nameof(GetTokenData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
        {
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }


        [Theory]
        [MemberData(nameof(GetTokenPairData))]
        public void Lexer_Lexes_Token_Pairs(
            SyntaxKind t1Kind, string t1Text,
            SyntaxKind t2Kind, string t2Text)
        {
            var tokens = SyntaxTree.ParseTokens(t1Text + t2Text).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, t2Kind);
            Assert.Equal(tokens[1].Text, t2Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairWithSeparatorData))]
        public void Lexer_Lexes_Token_Pairs_With_Separator(
            SyntaxKind t1Kind, string t1Text,
            SyntaxKind spKind, string spText,
            SyntaxKind t2Kind, string t2Text)
        {
            var tokens = SyntaxTree.ParseTokens(t1Text + spText+ t2Text).ToArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, spKind);
            Assert.Equal(tokens[1].Text, spText);
            Assert.Equal(tokens[2].Kind, t2Kind);
            Assert.Equal(tokens[2].Text, t2Text);
        }

        public static IEnumerable<object[]> GetTokenData()
        {
            foreach (var t in GetTokens())
                yield return new object[] { t.kind, t.text };
        }

        public static IEnumerable<object[]> GetTokenPairData()
        {
            foreach (var t in GetTokenPairs())
                yield return new object[] { t.t1Kind, t.t1Text, t.t2Kind, t.t2Text };
        }

        public static IEnumerable<object[]> GetTokenPairWithSeparatorData()
        {
            foreach (var t in GetTokenPairsWithSeparator())
                yield return new object[] { 
                    t.t1Kind, t.t1Text, 
                    t.spkind, t.spText, 
                    t.t2Kind, t.t2Text };
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            return new[] {
                //Literals Tests
                (SyntaxKind.CloseParenthesisToken, ")"),
                (SyntaxKind.OpenParenthesisToken, "("),
                (SyntaxKind.PlusToken, "+"),
                (SyntaxKind.MinusToken, "-"),
                (SyntaxKind.StarToken, "*"),
                (SyntaxKind.SlashToken, "/"),
                (SyntaxKind.PercentToken, "%"),
                (SyntaxKind.HatToken, "^"),            
                (SyntaxKind.TrueKeyword, "true"),
                (SyntaxKind.FalseKeyword, "false"),

                //Logical Tests
                (SyntaxKind.AssigmentToken, "="),
                (SyntaxKind.DoubleEqualsToken, "=="),
                (SyntaxKind.NotEqualsToken, "!="),
                (SyntaxKind.ExclamationToken, "!"),
                (SyntaxKind.LogicalOrToken, "||"),
                (SyntaxKind.LogicalAndToken, "&&"),
                (SyntaxKind.LessOrEqualsThenToken, "<="),
                (SyntaxKind.LessThenToken, "<"),
                (SyntaxKind.GreaterOrEqualsThenToken, ">="),
                (SyntaxKind.GreaterThenToken, ">"),

                //Space Tests
                (SyntaxKind.WhiteSpaceToken, " "),
                (SyntaxKind.WhiteSpaceToken, "  "),
                (SyntaxKind.WhiteSpaceToken, "\r"),
                (SyntaxKind.WhiteSpaceToken, "\n"),
                (SyntaxKind.WhiteSpaceToken, "\r\n"),

                //Number Tests
                (SyntaxKind.NumberToken, "1"),
                (SyntaxKind.NumberToken, "123456"),

                //Identifier Tests
                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                (SyntaxKind.IdentifierToken, "testing"),
            };
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[] {
                (SyntaxKind.WhiteSpaceToken, " "),
                (SyntaxKind.WhiteSpaceToken, "  "),
                (SyntaxKind.WhiteSpaceToken, "\r"),
                (SyntaxKind.WhiteSpaceToken, "\n"),
                (SyntaxKind.WhiteSpaceToken, "\r\n"),
            };
        }

        private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind) 
        =>  (
                (
                    (
                        t1Kind == SyntaxKind.IdentifierToken 
                            || 
                        t1Kind.ToString().EndsWith("Keyword")
                    ) 
                        && 
                    (
                        t2Kind == SyntaxKind.IdentifierToken 
                            || 
                        t2Kind.ToString().EndsWith("Keyword")
                    )
                ) 
                    ||
                (
                    (
                        t1Kind == SyntaxKind.ExclamationToken 
                            || 
                        t1Kind == SyntaxKind.AssigmentToken 
                            ||
                        t1Kind == SyntaxKind.LessThenToken 
                            || 
                        t1Kind == SyntaxKind.GreaterThenToken
                    ) 
                    && 
                    (
                        t2Kind == SyntaxKind.AssigmentToken 
                            || 
                        t2Kind == SyntaxKind.DoubleEqualsToken
                    )
                ) 
                    || 
                (
                    t1Kind == SyntaxKind.NumberToken 
                        && 
                    t2Kind == SyntaxKind.NumberToken
                )
            );

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
                foreach (var t2 in GetTokens())
                {
                    if (t1.kind == SyntaxKind.WhiteSpaceToken && 
                        t2.kind == SyntaxKind.WhiteSpaceToken)
                        continue;
                    if (!RequiresSeparator(t1.kind, t2.kind))
                        yield return (t1.kind, t1.text, t2.kind, t2.text);
                }
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind spkind, string spText, SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
        {
            foreach (var t1 in GetTokens())
                foreach (var t2 in GetTokens())
                {
                    if (t1.kind == SyntaxKind.WhiteSpaceToken && 
                        t2.kind == SyntaxKind.WhiteSpaceToken)
                        continue;
                    if (RequiresSeparator(t1.kind, t2.kind))
                        foreach (var s in GetSeparators())
                            yield return ( 
                                t1.kind, t1.text, 
                                s.kind, s.text, 
                                t2.kind, t2.text);
                }
        }
    }
}
