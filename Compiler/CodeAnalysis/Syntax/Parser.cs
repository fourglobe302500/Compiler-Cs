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

        public Parser ( SourceText text )
        {
            List<SyntaxToken> tokens = new List<SyntaxToken>();
            Lexer lexer = new Lexer(text);
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

        private SyntaxToken Peek ( int offset ) => _position + offset >= _tokens.Length ? _tokens[^1] : _tokens[_position + offset];

        private SyntaxToken Current => Peek(0);
        private SyntaxToken LookAhead => Peek(1);
        private SyntaxToken Last => Peek(-1);

        public SyntaxToken NextToken ( )
        {
            SyntaxToken current = Current;
            _position++;
            return current;
        }

        private SyntaxToken MatchToken ( SyntaxKind kind )
        {
            if (Current.Kind == kind)
                return NextToken();
            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public CompilationUnitSyntax ParseCompilationUnit ( ) => new CompilationUnitSyntax(ParseStatement(), MatchToken(SyntaxKind.EndOfFileToken));

        private StatementSyntax ParseStatement ( ) => Current.Kind == SyntaxKind.OpenBraceToken ?
            ParseBlockStatement() : (StatementSyntax)ParseExpressionStatement();

        private BlockStatementSyntax ParseBlockStatement ( )
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);

            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }

            var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

            return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private ExpressionStatementSyntax ParseExpressionStatement ( ) => new ExpressionStatementSyntax(ParseExpression());

        private ExpressionSyntax ParseExpression ( ) => ParseAssigmentExpression();

        private ExpressionSyntax ParseAssigmentExpression ( ) =>
            Current.Kind == SyntaxKind.IdentifierToken && LookAhead.Kind == SyntaxKind.AssigmentToken ?
                new AssigmentExpressionSyntax(NextToken(), NextToken(), ParseAssigmentExpression()) :
                ParseBinaryExpression();

        private ExpressionSyntax ParseBinaryExpression ( int parentPrecedence = 0 )
        {
            ExpressionSyntax left;
            int unaryOperatorPrecedence =
            Current.Kind.GetUnaryOperatorPrecedence();
            left = unaryOperatorPrecedence == 0 || unaryOperatorPrecedence < parentPrecedence
                ? ParsePrimaryExpression()
                : new UnaryExpressionSyntax(
                NextToken(),
                ParseBinaryExpression(unaryOperatorPrecedence));
            while (true)
            {
                int precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;
                left = new BinaryExpressionSyntax(
                left,
                NextToken(),
                ParseBinaryExpression(precedence));
            }
            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression ( ) => Current.Kind switch {
            SyntaxKind.OpenParenthesisToken => ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword => ParseBooleanLiteral(),
            SyntaxKind.FalseKeyword => ParseBooleanLiteral(),
            SyntaxKind.NumberToken => ParseNumberLiteral(),
            _ => ParseNameExpression(),
        };

        private ExpressionSyntax ParseParenthesizedExpression ( )
            => new ParenthesizedExpressionSyntax(
                MatchToken(SyntaxKind.OpenParenthesisToken),
                ParseExpression(),
                MatchToken(SyntaxKind.CloseParenthesisToken));

        private ExpressionSyntax ParseBooleanLiteral ( ) =>
            new LiteralExpressionSyntax(
                Current.Kind == SyntaxKind.TrueKeyword ? MatchToken(SyntaxKind.TrueKeyword) :
                        MatchToken(SyntaxKind.FalseKeyword),
                Last.Kind == SyntaxKind.TrueKeyword);

        private ExpressionSyntax ParseNumberLiteral ( )
            => new LiteralExpressionSyntax(MatchToken(SyntaxKind.NumberToken));

        private ExpressionSyntax ParseNameExpression ( ) =>
            new NameExpressionSyntax(MatchToken(SyntaxKind.IdentifierToken));
    }
}