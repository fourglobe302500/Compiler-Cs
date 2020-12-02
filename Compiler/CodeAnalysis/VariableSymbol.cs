using System;
namespace Compiler.CodeAnalysis
{
    public sealed class VariableSymbol
    {
        internal VariableSymbol(string name, bool isReadOnly, Type type)
        {
            Name = name;
            IsReadOnly = isReadOnly;
            Type = type;
        }
        public bool IsReadOnly { get; }
        public Type Type { get; }
        public string Name { get; }
        public override string ToString( ) => Name;
    }
}