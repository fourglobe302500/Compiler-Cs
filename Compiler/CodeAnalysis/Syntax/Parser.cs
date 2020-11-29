using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
        private SyntaxToken Peek(int offset) => _position + offset >= _tokens.Length ? _tokens[^1] : _tokens[_position + offset];
        private SyntaxToken Current => Peek(0);
        private SyntaxToken LookAhead => Peek(1);
        private SyntaxToken Last => Peek(-1);
        public SourceText Text => _text;
        public SyntaxToken NextToken( )
        {
            SyntaxToken current = Current;
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
        public CompilationUnitSyntax ParseCompilationUnit( ) => new CompilationUnitSyntax(ParseStatement(), MatchToken(SyntaxKind.EndOfFileToken));
        private StatementSyntax ParseStatement( ) => Current.Kind switch {
            SyntaxKind.OpenBraceToken => ParseBlockStatement(),
            var kind when new[] { SyntaxKind.VarKeyword, SyntaxKind.DefKeyword }.Contains(kind) => ParseVariableDeclaration(),
            SyntaxKind.IfKeyword => ParseIfStatement(),
            SyntaxKind.WhileKeyword => ParseWhileStatement(),
            SyntaxKind.ForKeyword => ParseForStatement(),
            _ => ParseExpressionStatement()
        };
        private BlockStatementSyntax ParseBlockStatement( )
        {
            ImmutableArray<StatementSyntax>.Builder statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            SyntaxToken openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            var startToken = Current;
            while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                StatementSyntax statement = ParseStatement();
                statements.Add(statement);
                if (Current == startToken)
                    NextToken();
                startToken = Current;
            }
            return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), MatchToken(SyntaxKind.CloseBraceToken));
        }
        private ExpressionStatementSyntax ParseExpressionStatement( ) => new ExpressionStatementSyntax(ParseExpression());
        private VariableDeclarationSyntax ParseVariableDeclaration( )
            => new VariableDeclarationSyntax(MatchToken(Current.Kind == SyntaxKind.VarKeyword ? SyntaxKind.VarKeyword : SyntaxKind.DefKeyword),
                                             MatchToken(SyntaxKind.IdentifierToken),
                                             MatchToken(SyntaxKind.AssigmentToken),
                                             ParseExpression());
        private IfStatementSyntax ParseIfStatement( )
        {
            var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var thenStatement = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatementSyntax(ifKeyword, condition, thenStatement, elseClause);
        }
        private WhileStatementSyntax ParseWhileStatement( )
        {
            var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var whileStatement = ParseStatement();
            return new WhileStatementSyntax(whileKeyword, condition, whileStatement);
        }
        private ForStatementSyntax ParseForStatement( )
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var declarationStatement = ParseStatement
                ();
            var condition = ParseExpression();
            var incrementExpression = ParseExpression();
            var forStatement = ParseStatement();
            return new ForStatementSyntax(forKeyword, declarationStatement, condition, incrementExpression, forStatement);
        }
#nullable enable
        private ElseClauseSyntax? ParseElseClause( )
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;
            var keyword = NextToken();
            var code = ParseStatement();
            return new ElseClauseSyntax(keyword, code);
        }
        private ExpressionSyntax ParseExpression( ) => ParseAssigmentExpression();
        private ExpressionSyntax ParseAssigmentExpression( ) =>
            Current.Kind == SyntaxKind.IdentifierToken && LookAhead.Kind == SyntaxKind.AssigmentToken ?
                new AssigmentExpressionSyntax(NextToken(), NextToken(), ParseAssigmentExpression()) :
                ParseBinaryExpression();
        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
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
        private ExpressionSyntax ParsePrimaryExpression( ) => Current.Kind switch {
            SyntaxKind.OpenParenthesisToken => ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword => ParseBooleanLiteral(),
            SyntaxKind.FalseKeyword => ParseBooleanLiteral(),
            SyntaxKind.NumberToken => ParseNumberLiteral(),
            _ => ParseNameExpression(),
        };
        private ExpressionSyntax ParseParenthesizedExpression( )
            => new ParenthesizedExpressionSyntax(
                MatchToken(SyntaxKind.OpenParenthesisToken),
                ParseExpression(),
                MatchToken(SyntaxKind.CloseParenthesisToken));
        private ExpressionSyntax ParseBooleanLiteral( ) =>
            new LiteralExpressionSyntax(
                Current.Kind == SyntaxKind.TrueKeyword ? MatchToken(SyntaxKind.TrueKeyword) :
                        MatchToken(SyntaxKind.FalseKeyword),
                Last.Kind == SyntaxKind.TrueKeyword);
        private ExpressionSyntax ParseNumberLiteral( )
            => new LiteralExpressionSyntax(MatchToken(SyntaxKind.NumberToken));
        private ExpressionSyntax ParseNameExpression( ) =>
            new NameExpressionSyntax(MatchToken(SyntaxKind.IdentifierToken));
    }
}