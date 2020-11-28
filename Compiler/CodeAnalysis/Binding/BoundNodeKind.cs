namespace Compiler.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        //Statements
        BlockStatement,
        ExpressionStatement,

        //Expressions
        LiteralToken,
        VariableExpression,
        AssigmentExpression,
        UnaryExpression,
        BinaryExpression,
    }
}