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
    private SyntaxToken LookAhead => Peek(1);

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

    private ExpressionSyntax ParseExpression()
      => ParseAssigmentExpression();

    private ExpressionSyntax ParseAssigmentExpression() 
      =>  Current.Kind == SyntaxKind.IdentifierToken &&
          LookAhead.Kind == SyntaxKind.AssigmentToken ?
            new AssigmentExpressionSyntax(
              NextToken(), NextToken(), ParseAssigmentExpression()) :
            ParseBinaryExpression();

    private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
    {
      ExpressionSyntax left;
      var unaryOperatorPrecedence = 
        Current.Kind.GetUnaryOperatorPrecedence();
      if (unaryOperatorPrecedence == 0 ||
          unaryOperatorPrecedence < parentPrecedence)
        left = ParsePrimaryExpression();
      else
        left = new UnaryExpressionSyntax(
          NextToken(),
          ParseBinaryExpression(unaryOperatorPrecedence));
      while (true)
      {
        var precedence = Current.Kind.GetBinaryOperatorPrecedence();
        if (precedence == 0 || precedence <= parentPrecedence)
          break;
        left = new BinaryExpressionSyntax(
          left,
          NextToken(),
          ParseBinaryExpression(precedence));
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
        case SyntaxKind.FalseKeyword: 
          return new LiteralExpressionSyntax(
            Current, NextToken().Kind == SyntaxKind.TrueKeyword);
        case SyntaxKind.IdentifierToken:
          return new NameExpressionSyntax(NextToken());
        default: 
          return new LiteralExpressionSyntax(MatchToken(SyntaxKind.NumberToken));
      }
    }
  }
}