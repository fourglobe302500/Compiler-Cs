using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler
{
    internal static class Program
    {
        private static void Main()
        {
            bool showTree = false;
            var variables = new Dictionary<VariableSymbol, object>();
            var textBuilder = new StringBuilder();

            while (true)
            {
                if (textBuilder.Length == 0)
                    Console.Write("> ");
                else
                    Console.Write("| ");
                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);
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
                    }
                }

                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();
                var syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Diagnostics.Any())
                    continue;

                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);
                var diagnostics = result.Diagnostics;
                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }
                if (!diagnostics.Any())
                    Console.WriteLine(result.Value);
                else
                {
                    foreach (var err in diagnostics)
                    {
                        var lineIndex = syntaxTree.Text.GetLineIndex(err.Span.Start);
                        var line = syntaxTree.Text.Lines[lineIndex];
                        var lineNumber = lineIndex + 1;
                        var character = err.Span.Start - line.Start + 1;

                        var prefixSpan = TextSpan.FromBounds(line.Start, err.Span.Start);
                        var suffixSpan = TextSpan.FromBounds(err.Span.End, line.End);

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
                textBuilder.Clear();
            }
        }
    }
}