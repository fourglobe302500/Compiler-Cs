namespace Compiler.CodeAnalysis.Syntax
{
  internal sealed class SyntaxFacts
  {
    public static int GetUnaryOperatorPrecedence(SyntaxKind kind) 
    => kind switch 
    {
      SyntaxKind.PlusToken => 7,
      SyntaxKind.MinusToken => 7,
      SyntaxKind.ExclamationToken => 7,
      _ => 0
    };
    public static int GetBinaryOperatorPrecedence(SyntaxKind kind) 
    => kind switch
    {
      SyntaxKind.PercentToken => 6,
      SyntaxKind.HatToken => 6,
      SyntaxKind.SlashToken => 5,
      SyntaxKind.StarToken => 5,
      SyntaxKind.MinusToken => 4,
      SyntaxKind.PlusToken => 4,
      SyntaxKind.DoubleEqualsToken => 3,
      SyntaxKind.NotEqualsToken => 3,
      SyntaxKind.LessOrEqualsThenToken => 3,
      SyntaxKind.LessThenToken => 3,
      SyntaxKind.GreaterOrEqualsThenToken => 3,
      SyntaxKind.GreaterThenToken => 3,
      SyntaxKind.LogicalAndToken => 2,
      SyntaxKind.LogicalOrToken => 1,
      _ => 0
    };
    public static SyntaxKind GetKeywordKind(string text) => text switch
    {
      "true" => SyntaxKind.TrueKeyword,
      "false" => SyntaxKind.FalseKeyword,
      _ => SyntaxKind.IndentifierToken
    };
  }
}