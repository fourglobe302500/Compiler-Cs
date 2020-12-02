using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Lowering;
using Compiler.CodeAnalysis.Syntax;
namespace Compiler.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope _globalScope;
        public Compilation(SyntaxTree syntax) : this(null, syntax)
        {
        }
        private Compilation(Compilation previous, SyntaxTree syntax)
        {
            Previous = previous;
            Syntax = syntax;
        }
        public Compilation Previous { get; }
        public SyntaxTree Syntax { get; }
        internal BoundGlobalScope GlobalScope
        {
            get {
                if (_globalScope == null)
                {
                    BoundGlobalScope globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, Syntax.Root);
                    _ = Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }
                return _globalScope;
            }
        }
        public Compilation ContinueWith(SyntaxTree syntaxTree) => new Compilation(this, syntaxTree);
        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
            => Syntax.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray().Any()
                ? new EvaluationResult(Syntax.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray(), null)
                : new EvaluationResult(ImmutableArray<Diagnostic>.Empty, new Evaluator(GetStatement(), variables).Evaluate());
        public void EmitTree(TextWriter writer)
        {
            GetStatement().WriteTo(writer);
        }

        private BoundBlockStatement GetStatement( ) => Lowerer.Lower(GlobalScope.Statement);
    }
}