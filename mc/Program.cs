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
      var variables = new Dictionary<string, object>();
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
          PrettyPrint(syntaxTree.Root);
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

    static void PrettyPrint(SyntaxNode node, string indent = "", bool last = true)
    {
      Console.Write(value: $"{indent}{(last ? "└──" : "├──")}{node.Kind}");
      if (node is SyntaxToken t && t.Value != null)
        Console.Write($": {t.Value}");
      indent += last ? "   " : "│  ";
      Console.WriteLine();
      foreach (var child in node.GetChildren())
        PrettyPrint(child, indent, child == node.GetChildren().LastOrDefault());
    }
  }
}
