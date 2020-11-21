using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.CodeAnalysis.Text;

namespace Compiler.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;

        private int _position;

        public Parser(SourceText text)
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
            _text = text;
        }

        public DiagnosticBag Diagnostics => _diagnostics;
        private SyntaxToken Peek(int offset) => _position + offset
            >= _tokens.Length ? _tokens[_tokens.Length - 1] :
            _tokens[_position + offset];
        private SyntaxToken Current => Peek(0);
        private SyntaxToken LookAhead => Peek(1);
        private SyntaxToken Last => Peek(-1);

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

        public SyntaxTree Parse()
        {
            ExpressionSyntax root = ParseExpression();
            SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_text, Diagnostics.ToImmutableArray(), root, endOfFileToken);
        }

        private ExpressionSyntax ParseExpression() => ParseAssigmentExpression();

        private ExpressionSyntax ParseAssigmentExpression()=> 
            Current.Kind == SyntaxKind.IdentifierToken && LookAhead.Kind == SyntaxKind.AssigmentToken ?
                new AssigmentExpressionSyntax(NextToken(), NextToken(), ParseAssigmentExpression()) :
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
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesizedExpression();
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral();
                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();
                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameExpression();
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
            => new ParenthesizedExpressionSyntax(
                MatchToken(SyntaxKind.OpenParenthesisToken), 
                ParseExpression(), 
                MatchToken(SyntaxKind.CloseParenthesisToken));

        private ExpressionSyntax ParseBooleanLiteral() => 
            new LiteralExpressionSyntax(
                Current.Kind == SyntaxKind.TrueKeyword ? MatchToken(SyntaxKind.TrueKeyword) :
                        MatchToken(SyntaxKind.FalseKeyword),
                Last.Kind == SyntaxKind.TrueKeyword);

        private ExpressionSyntax ParseNumberLiteral()
            => new LiteralExpressionSyntax(MatchToken(SyntaxKind.NumberToken));

        private ExpressionSyntax ParseNameExpression() => 
            new NameExpressionSyntax(MatchToken(SyntaxKind.IdentifierToken));
    }
}