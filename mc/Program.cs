using System.Linq;
using System;

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Binding;

namespace Compiler
{
  internal static class Program
  {
    private static void Main()
    {
      bool showTree = false;
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
            "Showing the parsed trees" : "Not showing the parsed trees");
            continue;
          case "#cls":
            Console.Clear();
            continue;
        }

        var syntaxTree = SyntaxTree.Parse(line);
        var binder = new Binder();
        var boundExpression = binder.BindExpression(syntaxTree.Root);
        var diagnostics = 
          syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();
        if (showTree)
        {
          Console.ForegroundColor = ConsoleColor.DarkGray;
          PrettyPrint(syntaxTree.Root);
        }
        if (!diagnostics.Any())
          Console.WriteLine(new Evaluator(boundExpression).Evaluate);
        else
        {
          Console.ForegroundColor = ConsoleColor.DarkRed;
          foreach (var err in diagnostics)
            Console.WriteLine(err);
          Console.ResetColor();
        }
      }
    }
    
    static void PrettyPrint(SyntaxNode node, string indent = "",bool last = true )
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
