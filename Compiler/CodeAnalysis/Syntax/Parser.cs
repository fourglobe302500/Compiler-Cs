using System.Collections.Generic;

namespace Compiler.CodeAnalysis.Syntax
{
  internal sealed class Parser
  {
    private readonly SyntaxToken[] _tokens;
    private int _position;
    private DiagnosticBag _diagnostics = new DiagnosticBag();

    public Parser(string text)
    {
      var tokens = new List<SyntaxToken>();
      var lexer = new Lexer(text); 
      SyntaxToken token;
      do
      {
        token = lexer.Lex();
        switch (token.Kind)
        {
          case SyntaxKind.WhiteSpaceToken:
          case SyntaxKind.InvalidToken:
            continue;
        }
        tokens.Add(token);
      } while (token.Kind != SyntaxKind.EndOfFileToken);
      _tokens = tokens.ToArray();
      _diagnostics.AddRange(lexer.Diagnostics);
    }

    public DiagnosticBag Diagnostics => _diagnostics;
    private SyntaxToken Peek(int offset) => _position + offset
      >= _tokens.Length ? _tokens[_tokens.Length - 1] :
      _tokens[_position + offset];
    private SyntaxToken Current => Peek(0);

    public SyntaxToken NextToken()
    {
      var current = Current;
      _position++;
      return current;
    }

    private SyntaxToken MatchToken(SyntaxKind kind)
    {
      if (Current.Kind == kind)
        return NextToken();
      _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
      return new SyntaxToken(kind, Current.Position, null, null);
    }

    public SyntaxTree Parse => new SyntaxTree(
      Diagnostics,
      ParseExpression(),
      MatchToken(SyntaxKind.EndOfFileToken));

    private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
    {
      ExpressionSyntax left;
      var unaryOperatorPrecedence = 
        SyntaxFacts.GetUnaryOperatorPrecedence(Current.Kind);
      if (unaryOperatorPrecedence == 0 ||
          unaryOperatorPrecedence < parentPrecedence)
        left = ParsePrimaryExpression();
      else
        left = new UnaryExpressionSyntax(
          NextToken(),
          ParseExpression(unaryOperatorPrecedence));
      while (true)
      {
        var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(Current.Kind);
        if (precedence == 0 || precedence <= parentPrecedence)
          break;
        left = new BinaryExpressionSyntax(
          left,
          NextToken(),
          ParseExpression(precedence));
      }
      return left;
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
      switch(Current.Kind)
      {
        case SyntaxKind.OpenParenthesisToken:
          return new ParenthesizedExpressionSyntax(
            NextToken(),
            ParseExpression(),
            MatchToken(SyntaxKind.CloseParenthesisToken));
        case SyntaxKind.TrueKeyword:  
          return new LiteralExpressionSyntax(NextToken(), true);
        case SyntaxKind.FalseKeyword: 
          return new LiteralExpressionSyntax(NextToken(), false);
        default: 
          return new LiteralExpressionSyntax(MatchToken(SyntaxKind.NumberToken));
      }
    }
  }
}