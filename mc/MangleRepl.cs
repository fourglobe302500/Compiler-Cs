using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Symbols;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;

namespace Compiler
{
    internal sealed class MangleRepl : Repl
    {
        private bool _showTree;
        private bool _showProgram;
        private Compilation _state;
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();

        protected override void RenderLine(string line)
        {
            if (line.StartsWith('#'))
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write(line);
                return;
            }
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                Console.ForegroundColor = token.Kind switch {
                    var keyKind when SyntaxFacts.GetKeywords().Contains(keyKind) => ConsoleColor.Blue,
                    SyntaxKind.IdentifierToken => ConsoleColor.DarkYellow,
                    SyntaxKind.NumberToken => ConsoleColor.Magenta,
                    SyntaxKind.InvalidToken => ConsoleColor.Red,
                    _ => ConsoleColor.Cyan,
                };
                Console.Write(token.Text);
            }
        }

        protected override void EvaluateMetaCommand(string input)
        {
            Console.ForegroundColor = ConsoleColor.White;
            switch (input)
            {
                case "#toogleTree":
                    _showTree = !_showTree;
                    Console.WriteLine(_showTree ?
                        "Showing the parsed trees." :
                        "Not showing the parsed trees.");
                    break;
                case "#toogleProgram":
                    _showProgram = !_showProgram;
                    Console.WriteLine(_showProgram ?
                        "Showing the bound tree." :
                        "Not showing the bound tree.");
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    _state = null;
                    ClearHistory();
                    break;
                case "#clearHistory":
                    ClearHistory();
                    break;
                default:
                    base.EvaluateMetaCommand(input);
                    break;
            }
        }

        protected override bool IsCompleteSubmission(string text)
            => string.IsNullOrEmpty(text)
               || text.Split(Environment.NewLine).Reverse().TakeWhile(s => string.IsNullOrEmpty(s)).Count() >= 2
               || !SyntaxTree.Parse(text).Root.Statement.GetLastToken().IsMissing;

        protected override void EvaluateSubmission(string text)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);

            Compilation compilation = _state == null ? new Compilation(syntaxTree) : _state.ContinueWith(syntaxTree);
            if (_showTree)
                syntaxTree.Root.WriteTo(Console.Out);
            if (_showProgram)
                compilation.EmitTree(Console.Out);
            EvaluationResult result = compilation.Evaluate(_variables);
            ImmutableArray<Diagnostic> diagnostics = result.Diagnostics;
            if (!diagnostics.Any())
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(result.Value);
                Console.ResetColor();
                _state = compilation;
            }
            else
            {
                foreach (
                    (Diagnostic err, int lineNumber, int character, TextSpan prefixSpan, TextSpan suffixSpan) in from err in diagnostics
                                                                                                                 let lineIndex = syntaxTree.Text.GetLineIndex(err.Span.Start)
                                                                                                                 let line = syntaxTree.Text.Lines[lineIndex]
                                                                                                                 let lineNumber = lineIndex + 1
                                                                                                                 let character = err.Span.Start - line.Start + 1
                                                                                                                 let prefixSpan = TextSpan.FromBounds(line.Start, err.Span.Start)
                                                                                                                 let suffixSpan = TextSpan.FromBounds(err.Span.End, line.End)
                                                                                                                 select (err, lineNumber, character, prefixSpan, suffixSpan))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"({lineNumber},{character}): {err} ");
                    Console.ResetColor();
                    Console.Write($"    {syntaxTree.Text.ToString(prefixSpan)}");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(syntaxTree.Text.ToString(err.Span));
                    Console.ResetColor();
                    Console.WriteLine(syntaxTree.Text.ToString(suffixSpan));
                }

                Console.WriteLine();
            }
        }
    }
}