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
        SemiColonToken,

        //Operators
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        PercentToken,
        HatToken,
        PipeToken,
        AmpersandToken,
        TildeToken,

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
        ElseKeyword,
        IfKeyword,
        ForKeyword,
        WhileKeyword,

        //Logical Operators
        PipePipeToken,
        AmpersandAmpersandToken,
        NotEqualsToken,
        DoubleEqualsToken,
        LessOrEqualsThenToken,
        LessThenToken,
        GreaterOrEqualsThenToken,
        GreaterThenToken,

        //Node
        CompilationUnit,
        ElseClause,

        //Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,
        IfStatement,
        WhileStatement,
        ForStatement,
    }
}