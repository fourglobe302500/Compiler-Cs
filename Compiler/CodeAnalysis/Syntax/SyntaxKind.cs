namespace Compiler.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        //TODO:
        //Tokens
        EndOfFileToken,

        InvalidToken,
        IdentifierToken,
        WhiteSpaceToken,
        NumberToken,
        CloseParenthesisToken,
        OpenParenthesisToken,
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

        //Logical Operators
        LogicalOrToken,

        LogicalAndToken,
        NotEqualsToken,
        DoubleEqualsToken,
        LessOrEqualsThenToken,
        LessThenToken,
        GreaterOrEqualsThenToken,
        GreaterThenToken,
    }
}