namespace Compiler.CodeAnalysis.Syntax
{
  internal sealed class Lexer
  {
    private readonly string _text;
    private int _position;
    private DiagnosticBag _diagnostics = new DiagnosticBag();

    public Lexer(string text) => _text = text;
    public DiagnosticBag Diagnostics => _diagnostics;
    private char Current => Peek(0);
    private char Lookahead => Peek(1);
    private char Peek(int offset) => 
      _position + offset >= _text.Length ? '\0' : _text[_position + offset];
    private char Next() => _text[_position++];
    private int Walk(int Dist) => (_position += Dist) - Dist;

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
          _diagnostics.ReportInvalidNumber(
            new TextSpan(start, length), _text, typeof(int));
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
          new SyntaxToken(SyntaxKind.LogicalOrToken, Walk(2), "||") : 
          new SyntaxToken(SyntaxKind.InvalidToken, _position++, "|");
        case '&': return Lookahead == '&' ? 
          new SyntaxToken(SyntaxKind.LogicalAndToken, Walk(2), "&&") : 
          new SyntaxToken(SyntaxKind.InvalidToken, _position++, "&");
        case '!': return Lookahead == '=' ?
          new SyntaxToken(SyntaxKind.NotEqualsToken, Walk(2), "!=") :
          new SyntaxToken(SyntaxKind.ExclamationToken, _position++, "!");
        case '=': return Lookahead == '=' ? 
          new SyntaxToken(SyntaxKind.DoubleEqualsToken, Walk(2), "==") :
          new SyntaxToken(SyntaxKind.AssigmentToken, _position++, "=");
        case '<': return Lookahead == '=' ?
          new SyntaxToken(SyntaxKind.LessOrEqualsThenToken, Walk(2), "<=") :
          new SyntaxToken(SyntaxKind.LessThenToken, _position++, "<");
        case '>': return Lookahead == '=' ?
          new SyntaxToken(SyntaxKind.GreaterOrEqualsThenToken, Walk(2), ">=") :
          new SyntaxToken(SyntaxKind.GreaterThenToken, _position++, ">");
        default: 
          _diagnostics.ReportInvalidCharacter(_position, Current);
          return new SyntaxToken(SyntaxKind.InvalidToken, _position++, "\0"); 
      }
    }
  }
}