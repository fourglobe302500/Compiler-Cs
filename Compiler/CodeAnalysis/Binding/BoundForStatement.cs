namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public BoundForStatement(BoundStatement declarationStatement, BoundExpression condition, BoundExpression increment, BoundStatement forStatement)
        {
            DeclarationStatement = declarationStatement;
            Condition = condition;
            Increment = increment;
            ForStatement = forStatement;
        }
        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        public BoundStatement DeclarationStatement { get; }
        public BoundExpression Condition { get; }
        public BoundExpression Increment { get; }
        public BoundStatement ForStatement { get; }
    }
}