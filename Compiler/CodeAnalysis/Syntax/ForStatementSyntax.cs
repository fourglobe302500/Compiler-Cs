namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // for var i = 0 i < 10 i = i + 1
        public ForStatementSyntax(SyntaxToken forKeyword, SyntaxToken openParenthesisToken, StatementSyntax declaritionStatement,
                                  ExpressionSyntax condition, SyntaxToken middleSemiColonToken, ExpressionSyntax incrementExpression,
                                  SyntaxToken closeParenthesisToken, StatementSyntax forStatement)
        {
            ForKeyword = forKeyword;
            OpenParenthesisToken = openParenthesisToken;
            DeclaritionStatement = declaritionStatement;
            Condition = condition;
            MiddleSemiColonToken = middleSemiColonToken;
            IncrementExpression = incrementExpression;
            CloseParenthesisToken = closeParenthesisToken;
            ForStatement = forStatement;
        }
        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken ForKeyword { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public StatementSyntax DeclaritionStatement { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken MiddleSemiColonToken { get; }
        public ExpressionSyntax IncrementExpression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public StatementSyntax ForStatement { get; }
    }
}