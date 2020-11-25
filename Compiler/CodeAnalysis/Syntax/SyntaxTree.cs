using Compiler.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(
            SourceText text,
            ImmutableArray<Diagnostic> diagnostics,
            ExpressionSyntax root,
            SyntaxToken endOfFileToken)
        {
            Text = text;
            Diagnostics = diagnostics;
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }

        public static SyntaxTree Parse(string text) =>
            Parse(SourceText.From(text));

        public static SyntaxTree Parse(SourceText text) =>
            new Parser(text).Parse();

        public static IEnumerable<SyntaxToken> ParseTokens(string text) =>
            ParseTokens(SourceText.From(text));

        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            Lexer lexer = new Lexer(text);
            while (true)
            {
                SyntaxToken token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;
                yield return token;
            }
        }
    }
}