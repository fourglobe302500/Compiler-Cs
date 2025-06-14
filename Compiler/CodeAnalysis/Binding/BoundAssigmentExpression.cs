
using Compiler.CodeAnalysis.Symbols;

namespace Compiler.CodeAnalysis.Binding
{
    internal class BoundAssigmentExpression : BoundExpression
    {
        public BoundAssigmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssigmentExpression;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public override TypeSymbol Type => Expression.Type;
    }
}