namespace Compiler.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        //Tokens
        EndOfFileToken,
        InvalidToken,
        IdentifierToken,
        WhiteSpaceToken,
        NumberToken,
        CloseParenthesisToken,
        OpenParenthesisToken,
        OpenBraceToken,
        CloseBraceToken,
        ExclamationToken,
        AssigmentToken,

        //Operators
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        PercentToken,
        HatToken,

        //Expressions
        LiteralExpression,
        NameExpression,
        AssigmentExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,

        //Keywords
        TrueKeyword,
        FalseKeyword,
        VarKeyword,
        DefKeyword,

        //Logical Operators
        LogicalOrToken,
        LogicalAndToken,
        NotEqualsToken,
        DoubleEqualsToken,
        LessOrEqualsThenToken,
        LessThenToken,
        GreaterOrEqualsThenToken,
        GreaterThenToken,

        //Node
        CompilationUnit,

        //Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,
    }
}