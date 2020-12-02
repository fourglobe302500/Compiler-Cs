using System;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement(BoundStatement node) => node.Kind switch {
            BoundNodeKind.BlockStatement => RewriteBlockStatement((BoundBlockStatement)node),
            BoundNodeKind.VariableDeclaration => RewriteVariableDeclaration((BoundVariableDeclaration)node),
            BoundNodeKind.IfStatement => RewriteIfStatement((BoundIfStatement)node),
            BoundNodeKind.WhileStatement => RewriteWhileStatement((BoundWhileStatement)node),
            BoundNodeKind.ForStatement => RewriteForStatement((BoundForStatement)node),
            BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStatement)node),
            BoundNodeKind.GotoStatement => RewriteGotoStatement((BoundGotoStatement)node),
            BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node),
            BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement)node),
            _ => throw new Exception($"Unexpected node: {node.Kind}"),
        };
        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;
            for (var i = 0; i < node.Statements.Length; i++)
            {
                var oldStatement = node.Statements[i];
                var newStatement = RewriteStatement(oldStatement);
                if (oldStatement != newStatement && builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);
                    for (int j = 0; j < i; j++)
                        builder.Add(node.Statements[j]);
                }
                if (builder != null)
                    builder.Add(newStatement);
            }
            return builder == null ? node : new BoundBlockStatement(builder.MoveToImmutable());
        }
        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
        {
            var initializer = RewriteExpression(node.Initializer);
            return initializer == node.Initializer ? node : new BoundVariableDeclaration(node.Variable, initializer);
        }
        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenStatement = RewriteStatement(node.ThenStatement);
            var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
            return condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement
                ? node : new BoundIfStatement(condition, thenStatement, elseStatement);
        }
        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var whileStatement = RewriteStatement(node.WhileStatement);
            return condition == node.Condition && whileStatement == node.WhileStatement ? node : new BoundWhileStatement(condition, whileStatement);
        }
        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var declarationStatement = RewriteStatement(node.DeclarationStatement);
            var condition = RewriteExpression(node.Condition);
            var increment = RewriteExpression(node.Increment);
            var forStatement = RewriteStatement(node.ForStatement);
            return declarationStatement == node.DeclarationStatement && condition == node.Condition && increment == node.Increment && forStatement == node.ForStatement
                ? node : new BoundForStatement(declarationStatement, condition, increment, forStatement);
        }
        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) => node;
        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) => node;
        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            return condition == node.Condition ? node : new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfTrue);
        }
        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expression = RewriteExpression(node.Expression);
            return expression == node.Expression ? node : new BoundExpressionStatement(expression);
        }
        public virtual BoundExpression RewriteExpression(BoundExpression node) => node.Kind switch {
            BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.AssigmentExpression => RewriteAssigmentExpression((BoundAssigmentExpression)node),
            BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression)node),
            _ => throw new Exception($"Unexpected node: {node.Kind}"),
        };
        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node) => node;
        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node) => node;
        protected virtual BoundExpression RewriteAssigmentExpression(BoundAssigmentExpression node)
        {
            var expression = RewriteExpression(node.Expression);
            return expression == node.Expression ? node : new BoundAssigmentExpression(node.Variable, expression);
        }
        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var operand = RewriteExpression(node.Operand);
            return operand == node.Operand ? node : new BoundUnaryExpression(node.Op, operand);
        }
        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);
            return left == node.Left && right == node.Right ? node : new BoundBinaryExpression(left, node.Op, right);
        }
    }
}