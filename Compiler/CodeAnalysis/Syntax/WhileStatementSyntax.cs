namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax whileStatement)
        {
            WhileKeyword = whileKeyword;
            Condition = condition;
            WhileStatement = whileStatement;
        }
        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax WhileStatement { get; }
    }
}