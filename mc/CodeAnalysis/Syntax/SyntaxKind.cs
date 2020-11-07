namespace Compiler.CodeAnalysis.Syntax
{
  public enum SyntaxKind
  {
    //TODO: 
    BitWaseAndToken,
    PipeToken,
    EqualsToken,

    //Tokens
    EndOfFileToken,
    InvalidToken,
    IndentifierToken,
    WhiteSpaceToken,
    NumberToken,
    CloseParenthesisToken,
    OpenParenthesisToken,
    ExclamationToken,

    //Operators
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    PercentToken,
    HatToken,

    //Expressions
    LiteralExpression,
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