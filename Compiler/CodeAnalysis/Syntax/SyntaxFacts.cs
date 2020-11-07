namespace Compiler.CodeAnalysis.Syntax
{
  internal sealed class SyntaxFacts
  {
    public static int GetUnaryOperatorPrecedence(SyntaxKind kind) 
    {
      switch(kind)
      {
        case SyntaxKind.PlusToken:
        case SyntaxKind.MinusToken:
        case SyntaxKind.ExclamationToken:
          return 7;
        default: return 0;
      }
    }
    public static int GetBinaryOperatorPrecedence(SyntaxKind kind) 
    {
      switch(kind)
      {
        case SyntaxKind.PercentToken:
        case SyntaxKind.HatToken: 
          return 6;
        case SyntaxKind.SlashToken:
        case SyntaxKind.StarToken: 
          return 5;
        case SyntaxKind.MinusToken:
        case SyntaxKind.PlusToken:
          return 4;
        case SyntaxKind.DoubleEqualsToken:
        case SyntaxKind.NotEqualsToken:
        case SyntaxKind.LessOrEqualsThenToken:
        case SyntaxKind.LessThenToken:
        case SyntaxKind.GreaterOrEqualsThenToken:
        case SyntaxKind.GreaterThenToken:
          return 3;
        case SyntaxKind.LogicalAndToken: 
          return 2;
        case SyntaxKind.LogicalOrToken:
          return 1;
        default: return 0;
      }
    }
    public static SyntaxKind GetKeywordKind(string text)
    {
      switch(text)
      {
        case "true": return SyntaxKind.TrueKeyword;
        case "false": return SyntaxKind.FalseKeyword;
        default: return SyntaxKind.IndentifierToken;
      }
    }
  }
}