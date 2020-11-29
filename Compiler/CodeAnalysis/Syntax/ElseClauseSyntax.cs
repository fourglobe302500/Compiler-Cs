namespace Compiler.CodeAnalysis.Syntax
{
    public sealed class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax code)
        {
            ElseKeyword = elseKeyword;
            Code = code;
        }
        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; }
        public StatementSyntax Code { get; }
    }
}