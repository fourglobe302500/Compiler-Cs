using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            Binder binder = new Binder(variables);
            BoundExpression boundExpression = binder.BindExpression(Syntax.Root.Expression);

            ImmutableArray<Diagnostic> diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            Evaluator evaluator = new Evaluator(boundExpression, variables);
            object value = evaluator.Evaluate;
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}