using System.Text;

using Compiler.CodeAnalysis.Text;

namespace Compiler.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly SourceText _text;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private int _position;
        private int _start;
        private SyntaxKind _kind;
        private object _value;

        public Lexer(SourceText text) => _text = text;
        public DiagnosticBag Diagnostics => _diagnostics;
        private char Current => Peek(0);
        private char Lookahead => Peek(1);

        private char Peek(int offset) =>
            _position + offset >= _text.Length ? '\0' : _text[_position + offset];

        private int Walk(int Dist) => (_position += Dist) - Dist;

        public SyntaxToken Lex( )
        {
            _start = _position;
            _kind = SyntaxKind.InvalidToken;
            _value = null;
            switch (Current)
            {
                case '\0': _kind = SyntaxKind.EndOfFileToken; _position--; break;
                case '+': _kind = SyntaxKind.PlusToken; break;
                case '-': _kind = SyntaxKind.MinusToken; break;
                case '*': _kind = SyntaxKind.StarToken; break;
                case '/': _kind = SyntaxKind.SlashToken; break;
                case '(': _kind = SyntaxKind.OpenParenthesisToken; break;
                case ')': _kind = SyntaxKind.CloseParenthesisToken; break;
                case '{': _kind = SyntaxKind.OpenBraceToken; break;
                case '}': _kind = SyntaxKind.CloseBraceToken; break;
                case '%': _kind = SyntaxKind.PercentToken; break;
                case '^': _kind = SyntaxKind.HatToken; break;
                case ';': _kind = SyntaxKind.SemiColonToken; break;
                case '~': _kind = SyntaxKind.TildeToken; break;
                case '|':
                    if (Lookahead == '|')
                    { _kind = SyntaxKind.PipePipeToken; _ = Walk(1); }
                    else
                        _kind = SyntaxKind.PipeToken;
                    break;
                case '&':
                    if (Lookahead == '&')
                    { _kind = SyntaxKind.AmpersandAmpersandToken; _ = Walk(1); }
                    else
                        _kind = SyntaxKind.AmpersandToken;
                    break;
                case '!':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.NotEqualsToken; _ = Walk(1); }
                    else
                        _kind = SyntaxKind.ExclamationToken;
                    break;
                case '=':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.DoubleEqualsToken; _ = Walk(1); }
                    else
                        _kind = SyntaxKind.AssigmentToken;
                    break;
                case '<':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.LessOrEqualsThenToken; _ = Walk(1); }
                    else
                        _kind = SyntaxKind.LessThenToken;
                    break;
                case '>':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.GreaterOrEqualsThenToken; _ = Walk(1); }
                    else
                        _kind = SyntaxKind.GreaterThenToken;
                    break;
                case var n when "0123456789".Contains(n):
                    ReadNumberToken();
                    _position--;
                    break;
                case '"':
                    ReadString();
                    _position--;
                    break;
                case var s when " \t\n\r".Contains(s):
                    ReadWhiteSpace();
                    _position--;
                    break;
                default:
                    if (char.IsLetter(Current))
                        ReadKeyword();
                    else if (char.IsWhiteSpace(Current))
                        ReadWhiteSpace();
                    else
                        _diagnostics.ReportInvalidCharacter(_position++, Current);
                    _position--;
                    break;
            }
            _position++;
            string text = SyntaxFacts.GetText(_kind);
            if (text == null)
                text = _text.ToString(_start, _position - _start);
            return new SyntaxToken(_kind, _start, text, _value);
        }

        private void ReadNumberToken( )
        {
            while (char.IsDigit(Current))
                _position++;
            int length = _position - _start;
            string text = _text.ToString(_start, length);
            if (!int.TryParse(text, out int value))
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, typeof(int));
            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void ReadString( )
        {
            _position++;
            var text = new StringBuilder();
            var done = false;
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        _diagnostics.ReportInvalidStringFormat(new TextSpan(_start, 1));
                        done = true;
                        break;
                    case '"':
                        _position++;
                        done = true;
                        break;
                    case '\\':
                        _position++;
                        switch (Current)
                        {
                            case '"':
                            case '\\':
                                text.Append(Current);
                                _position++;
                                break;
                        }
                        break;
                    default:
                        text.Append(Current);
                        _position++;
                        break;
                }
            }
            _value = text.ToString();
            _kind = SyntaxKind.StringToken;
        }

        private void ReadWhiteSpace( )
        {
            while (char.IsWhiteSpace(Current))
                _position++;
            _kind = SyntaxKind.WhiteSpaceToken;
        }

        private void ReadKeyword( )
        {
            while (char.IsLetter(Current))
                _position++;
            _kind = SyntaxFacts.GetKeywordKind(_text.ToString(_start, _position - _start));
        }
    }
}