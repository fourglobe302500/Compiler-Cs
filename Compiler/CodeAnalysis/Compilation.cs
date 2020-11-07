using System.Collections.Generic;
using System;
using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Syntax;
using System.Linq;

namespace Compiler.CodeAnalysis
{
  public sealed class Compilation
  {
    public Compilation(SyntaxTree syntax)
    {
      Syntax = syntax;
    }

    public SyntaxTree Syntax { get; }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
    { 
      var binder = new Binder(variables);
      var boundExpression = binder.BindExpression(Syntax.Root);

      var diagnostics = Syntax.Diagnostics
        .Concat(binder.Diagnostics).ToArray();
      if (diagnostics.Any())
        return new EvaluationResult(diagnostics, null);

      var evaluator = new Evaluator(boundExpression, variables);
      var value = evaluator.Evaluate;
      return new EvaluationResult(Array.Empty<Diagnostic>(), value);
    }
  }
}