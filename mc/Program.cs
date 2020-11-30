using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;
namespace Compiler
{
    internal static class Program
    {
        private static void Main( )
        {
            int LineNumber = 1;
            bool showTree = false;
            bool showProgram = false;
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
            StringBuilder textBuilder = new StringBuilder();
            Compilation previous = null;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (textBuilder.Length == 0)
                    Console.Write($"{string.Format("{0:00}", LineNumber++)}» ");
                else
                    Console.Write($"{string.Format("{0:00}", LineNumber++)}· ");

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
                                "Showing the parsed trees." :
                                "Not showing the parsed trees.");
                            LineNumber = 1;
                            continue;
                        case "#toogleProgram":
                            showProgram = !showProgram;
                            Console.WriteLine(showProgram ?
                                "Showing the bound tree." :
                                "Not showing the bound tree.");
                            LineNumber = 1;
                            continue;
                        case "#cls":
                            Console.Clear();
                            LineNumber = 1;
                            continue;
                        case "#reset":
                            previous = null;
                            LineNumber = 1;
                            continue;
                    }
                }

                _ = textBuilder.AppendLine(input);
                string text = textBuilder.ToString();
                SyntaxTree syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Diagnostics.Any())
                    continue;

                Compilation compilation = previous == null ? new Compilation(syntaxTree) : previous.ContinueWith(syntaxTree);
                EvaluationResult result = compilation.Evaluate(variables);
                ImmutableArray<Diagnostic> diagnostics = result.Diagnostics;
                if (showTree)
                    syntaxTree.Root.WriteTo(Console.Out);
                if (showProgram)
                    compilation.EmitTree(Console.Out);
                if (!diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($" => {result.Value}");
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
                LineNumber = 1;
                _ = textBuilder.Clear();
            }
        }
    }
}