namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // for var i = 0 i < 10 i = i + 1
        public ForStatementSyntax(SyntaxToken forKeyword, StatementSyntax declaritionStatement,
                                  ExpressionSyntax condition, ExpressionSyntax incrementExpression,
                                  StatementSyntax forStatement)
        {
            ForKeyword = forKeyword;
            DeclaritionStatement = declaritionStatement;
            Condition = condition;
            IncrementExpression = incrementExpression;
            ForStatement = forStatement;
        }
        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken ForKeyword { get; }
        public StatementSyntax DeclaritionStatement { get; }
        public ExpressionSyntax Condition { get; }
        public ExpressionSyntax IncrementExpression { get; }
        public StatementSyntax ForStatement { get; }
    }
}