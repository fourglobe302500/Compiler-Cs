using System;

using Compiler.CodeAnalysis.Symbols;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
            Type = Value switch {
                bool => TypeSymbol.Bool,
                int => TypeSymbol.Int,
                string => TypeSymbol.String,
                _ => throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}")
            };
        }

        public override TypeSymbol Type { get; }
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public object Value { get; }
    }
}