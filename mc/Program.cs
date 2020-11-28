using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Immutable;

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;

namespace Compiler
{
    internal static class Program
    {
        private static void Main ( )
        {
            bool showTree = false;
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
            StringBuilder textBuilder = new StringBuilder();
            Compilation previous = null;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (textBuilder.Length == 0)
                    Console.Write("» ");
                else
                    Console.Write("· ");

                Console.ForegroundColor = ConsoleColor.Gray;
                string input = Console.ReadLine();
                Console.ResetColor();

                bool isBlank = string.IsNullOrWhiteSpace(input);
                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                        break;
                    switch (input)
                    {
                        case "#toogleTree":
                            showTree = !showTree;
                            Console.WriteLine(showTree ?
                                "Showing the parsed trees" :
                                "Not showing the parsed trees");
                            continue;
                        case "#cls":
                            Console.Clear();
                            continue;
                        case "#reset":
                            previous = null;
                            continue;
                    }
                }

                _ = textBuilder.AppendLine(input);
                string text = textBuilder.ToString();
                SyntaxTree syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Diagnostics.Any())
                    continue;

                Compilation compilation = previous == null ?
                                          new Compilation(syntaxTree) :
                                          previous.ContinueWith(syntaxTree);
                EvaluationResult result = compilation.Evaluate(variables);
                ImmutableArray<Diagnostic> diagnostics = result.Diagnostics;
                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }
                if (!diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                    previous = compilation;
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
                _ = textBuilder.Clear();
            }
        }
    }
}