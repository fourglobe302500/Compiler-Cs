namespace Compiler.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        //Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,
        IfStatement,
        WhileStatement,

        //Expressions
        LiteralToken,
        VariableExpression,
        AssigmentExpression,
        UnaryExpression,
        BinaryExpression,
    }
}