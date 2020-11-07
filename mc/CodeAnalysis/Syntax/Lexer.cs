using System.Collections.Generic;

namespace Compiler.CodeAnalysis.Syntax
{
  internal sealed class Lexer
  {
    private readonly string _text;
    private int _position;
    private List<string> _diagnostics = new List<string>();

    public Lexer(string text) => _text = text;
    public IEnumerable<string> Diagnostics => _diagnostics;
    private char Current => Peek(0);
    private char Lookahead => Peek(1);
    private char Peek(int offset) => 
      _position + offset >= _text.Length ? '\0' : _text[_position + offset];
    private char Next() => _text[_position++];

    public SyntaxToken Lex()
    {
      if (_position >= _text.Length)
        return new SyntaxToken(SyntaxKind.EndOfFileToken, _text.Length, "\0", null);
      else if (char.IsDigit(Current))
      {
        var start = _position;

        while (char.IsDigit(Current))
          Next();

        var length = _position - start;
        var text = _text.Substring(start, length);
        if (!int.TryParse(text, out var value))
          _diagnostics.Add($"The number {_text} isn't an valid Int32");
        return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
      }
      else if (char.IsWhiteSpace(Current))
      {
        var start = _position;
        while (char.IsWhiteSpace(Current))
          Next();
        var length = _position - start;
        var text = _text.Substring(start, length);
        return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
      }
      else if (char.IsLetter(Current))
      {
        var start = _position;
        while (char.IsLetter(Current))
          Next();
        var length = _position - start;
        var text = _text.Substring(start, length);
        var kind = SyntaxFacts.GetKeywordKind(text);
        return new SyntaxToken(kind, start, text);
      }
      switch (Current)
      {
        case '+': return new SyntaxToken(
          SyntaxKind.PlusToken, _position++, "+");
        case '-': return new SyntaxToken(
          SyntaxKind.MinusToken, _position++, "-");
        case '*': return new SyntaxToken(
          SyntaxKind.StarToken, _position++, "*");
        case '/': return new SyntaxToken(
          SyntaxKind.SlashToken, _position++, "/");
        case '(': return new SyntaxToken(
          SyntaxKind.OpenParenthesisToken, _position++, "(");
        case ')': return new SyntaxToken(
          SyntaxKind.CloseParenthesisToken, _position++, ")");
        case '%': return new SyntaxToken(
          SyntaxKind.PercentToken, _position++, "%");
        case '^': return new SyntaxToken(
          SyntaxKind.HatToken, _position++, "^");
        case '|': return Lookahead == '|' ? 
          new SyntaxToken(SyntaxKind.LogicalOrToken, _position += 2, "||") : 
          new SyntaxToken(SyntaxKind.PipeToken, _position++, "|");
        case '&': return Lookahead == '&' ? 
          new SyntaxToken(SyntaxKind.LogicalAndToken, _position += 2, "&&") : 
          new SyntaxToken(SyntaxKind.BitWaseAndToken, _position++, "&");
        case '!': return Lookahead == '=' ?
          new SyntaxToken(SyntaxKind.NotEqualsToken, _position += 2, "!=") :
          new SyntaxToken(SyntaxKind.ExclamationToken, _position++, "!");
        case '=': return Lookahead switch
        {
          '=' => new SyntaxToken(SyntaxKind.DoubleEqualsToken, _position += 2, "=="),
          _ => new SyntaxToken(SyntaxToken.EqualsToken, _position++, "=")
        };
        case '<': return Lookahead == '=' ?
          new SyntaxToken(SyntaxKind.LessOrEqualsThenToken, _position += 2, "<=") :
          new SyntaxToken(SyntaxKind.LessThenToken, _position++, "<");
        case '>': return Lookahead == '=' ?
          new SyntaxToken(SyntaxKind.GreaterOrEqualsThenToken, _position += 2, ">=") :
          new SyntaxToken(SyntaxKind.GreaterThenToken, _position++, ">");
        default: 
          _diagnostics.Add($"ERROR: Invalid Token '{Current}'");
          return new SyntaxToken(
            SyntaxKind.InvalidToken, _position++, "\0"); 
      }
    }
  }

}