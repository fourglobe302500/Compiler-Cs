using System.Collections.Generic;
using System.Collections.Immutable;

using Compiler.CodeAnalysis.Binding;

namespace Compiler.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;

        private Lowerer( )
        {
        }
        private BoundLabel NewLabel => new BoundLabel($"Label{++_labelCount}");

        public static BoundBlockStatement Lower(BoundStatement statement) => Flatten(new Lowerer().RewriteStatement(statement));

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is BoundBlockStatement b)
                    for (var i = b.Statements.Length - 1; i >= 0; i--)
                        stack.Push(b.Statements[i]);
                else
                    builder.Add(current);
            }
            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = NewLabel;
                var endGotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition);
                var result = new BoundBlockStatement(
                    ImmutableArray.Create
                        (endGotoFalse, node.ThenStatement, new BoundLabelStatement(endLabel)));
                return RewriteStatement(result);
            }
            else
            {
                var endLabel = NewLabel;
                var elseLabel = NewLabel;
                var conditionalGotoStatement = new BoundConditionalGotoStatement(elseLabel, node.Condition);
                var endGoto = new BoundGotoStatement(endLabel);
                var result = new BoundBlockStatement(ImmutableArray.Create(
                    conditionalGotoStatement, node.ThenStatement,
                    endGoto, new BoundLabelStatement(elseLabel),
                    node.ElseStatement, new BoundLabelStatement(endLabel)));
                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var checkLabel = NewLabel;
            var loopLabel = NewLabel;
            var endLabel = NewLabel;
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var loopLabelStatement = new BoundLabelStatement(loopLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var checkGoto = new BoundGotoStatement(checkLabel);
            var loopGotoTrue = new BoundConditionalGotoStatement(loopLabel, node.Condition, true);
            var result = new BoundBlockStatement(ImmutableArray.Create(
                checkGoto, loopLabelStatement, node.WhileStatement, checkLabelStatement, loopGotoTrue, endLabelStatement));
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDeclaration = node.DeclarationStatement;
            var condition = node.Condition;
            var increment = new BoundExpressionStatement(node.Increment);
            var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.ForStatement, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBody);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));
            return RewriteStatement(result);
        }
    }
}