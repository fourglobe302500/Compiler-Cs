using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private readonly Dictionary<string, VariableSymbol> _variables = new Dictionary<string, VariableSymbol>();
        public BoundScope(BoundScope parent) => Parent = parent;
        public BoundScope Parent { get; }
        public bool TryDeclare(VariableSymbol variable)
        {
            if (_variables.ContainsKey(variable.Name))
                return false;
            _variables.Add(variable.Name, variable);
            return true;
        }
#pragma warning disable IDE0075 // Simplify conditional expression
        public bool TryLookup(string name, out VariableSymbol variable)
            => _variables.TryGetValue(name, out variable) ? true : Parent == null ? false : Parent.TryLookup(name, out variable);
#pragma warning restore IDE0075 // Simplify conditional expression
        public ImmutableArray<VariableSymbol> Variables => _variables.Values.ToImmutableArray();
    }
}