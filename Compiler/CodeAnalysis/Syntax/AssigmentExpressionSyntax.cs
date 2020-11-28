namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class AssigmentExpressionSyntax : ExpressionSyntax
    {
        public AssigmentExpressionSyntax(SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
        }
        public override SyntaxKind Kind => SyntaxKind.AssigmentExpression;
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Expression { get; }
    }
}