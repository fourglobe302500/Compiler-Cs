namespace Compiler.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        LiteralToken,
        VariableExpression,
        AssigmentExpression,
        UnaryExpression,
        BinaryExpression,
    }
}