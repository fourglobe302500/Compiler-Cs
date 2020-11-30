namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(ExpressionSyntax expression, SyntaxToken semiColonToken)
        {
            Expression = expression;
            SemiColonToken = semiColonToken;
        }
        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public ExpressionSyntax Expression { get; }
        public SyntaxToken SemiColonToken { get; }
    }
}