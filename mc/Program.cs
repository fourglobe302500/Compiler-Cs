using System.Collections.Generic;
using System.Linq;
using System;

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;

namespace Compiler
{
    internal static class Program
    {
        private static void Main()
        {
            bool showTree = false;
            var variables = new Dictionary<VariableSymbol, object>();
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    break;

                switch (line)
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

                var syntaxTree = SyntaxTree.Parse(line);
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
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(err);
                        Console.ResetColor();
                        Console.Write($"    {line.Substring(0, err.Span.Start)}");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(line.Substring(err.Span.Start, err.Span.Length));
                        Console.ResetColor();
                        Console.WriteLine(line.Substring(err.Span.End));
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
