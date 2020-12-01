using System.Collections.Immutable;

using Compiler.CodeAnalysis.Binding;

namespace Compiler.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private Lowerer( )
        {
        }
        public static BoundStatement Lower(BoundStatement statement) => new Lowerer().RewriteStatement(statement);
        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDeclaration = (BoundVariableDeclaration)node.DeclarationStatement;
            var condition = (BoundBinaryExpression)node.Condition;
            var increment = new BoundExpressionStatement(node.Increment);
            var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.ForStatement, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBody);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));
            return RewriteStatement(result);
        }
    }
}