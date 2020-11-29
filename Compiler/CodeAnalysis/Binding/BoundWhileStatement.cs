namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement whileStatement)
        {
            Condition = condition;
            WhileStatement = whileStatement;
        }
        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; }
        public BoundStatement WhileStatement { get; }
    }
}