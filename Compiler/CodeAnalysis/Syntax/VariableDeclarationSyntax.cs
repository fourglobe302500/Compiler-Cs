namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class VariableDeclarationSyntax : StatementSyntax
    {
        public VariableDeclarationSyntax(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalToken, ExpressionSyntax initializer, SyntaxToken semiColonToken)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualToken = equalToken;
            Initializer = initializer;
            SemiColonToken = semiColonToken;
        }
        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Initializer { get; }
        public SyntaxToken SemiColonToken { get; }
    }
}