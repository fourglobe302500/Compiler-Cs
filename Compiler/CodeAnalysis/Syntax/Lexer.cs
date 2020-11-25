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

        private char Next() => _text[_position++];

        private char Last => _text[Peek(-1)];

        private int Walk(int Dist) => (_position += Dist) - Dist;

        public SyntaxToken Lex()
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
                case '%': _kind = SyntaxKind.PercentToken; break;
                case '^': _kind = SyntaxKind.HatToken; break;
                case '|':
                    if (Lookahead == '|')
                    { _kind = SyntaxKind.LogicalOrToken; Walk(1); }
                    else
                        _kind = SyntaxKind.InvalidToken;
                    break;

                case '&':
                    if (Lookahead == '&')
                    { _kind = SyntaxKind.LogicalAndToken; Walk(1); }
                    else
                        _kind = SyntaxKind.InvalidToken;
                    break;

                case '!':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.NotEqualsToken; Walk(1); }
                    else
                        _kind = SyntaxKind.ExclamationToken;
                    break;

                case '=':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.DoubleEqualsToken; Walk(1); }
                    else
                        _kind = SyntaxKind.AssigmentToken;
                    break;

                case '<':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.LessOrEqualsThenToken; Walk(1); }
                    else
                        _kind = SyntaxKind.LessThenToken;
                    break;

                case '>':
                    if (Lookahead == '=')
                    { _kind = SyntaxKind.GreaterOrEqualsThenToken; Walk(1); }
                    else
                        _kind = SyntaxKind.GreaterThenToken;
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadNumberToken();
                    _position--;
                    break;

                case ' ':
                case '\t':
                case '\n':
                case '\r':
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

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
                _position++;
            int length = _position - _start;
            string text = _text.ToString(_start, length);
            if (!int.TryParse(text, out int value))
                _diagnostics.ReportInvalidNumber(
                    new TextSpan(_start, length), text, typeof(int));
            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                _position++;
            _kind = SyntaxKind.WhiteSpaceToken;
        }

        private void ReadKeyword()
        {
            while (char.IsLetter(Current))
                _position++;
            int length = _position - _start;
            string text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
        }
    }
}