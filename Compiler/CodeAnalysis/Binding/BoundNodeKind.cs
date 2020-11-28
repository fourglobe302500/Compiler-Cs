namespace Compiler.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        //Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,

        //Expressions
        LiteralToken,
        VariableExpression,
        AssigmentExpression,
        UnaryExpression,
        BinaryExpression,
    }
}