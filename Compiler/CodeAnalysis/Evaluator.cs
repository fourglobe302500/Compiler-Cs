using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Compiler.CodeAnalysis.Binding;
namespace Compiler.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly BoundStatement _root;
        private object _lastValue;
        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }
        public object Evaluate( )
        {
            EvaluateStatement(_root);
            return _lastValue;
        }
        private void EvaluateStatement(BoundStatement statement)
        {
            switch (statement.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement)statement);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)statement);
                    break;
                case BoundNodeKind.IfStatement:
                    EvaluateIfStatement((BoundIfStatement)statement);
                    break;
                case BoundNodeKind.WhileStatement:
                    EvaluateWhileStatement((BoundWhileStatement)statement);
                    break;
                case BoundNodeKind.ForStatement:
                    EvaluateForStatement((BoundForStatement)statement);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)statement);
                    break;
                default:
                    throw new Exception($"Unexpected node {statement.Kind}");
            }
        }
        private void EvaluateBlockStatement(BoundBlockStatement statement) => statement.Statements.ToImmutableList().ForEach(s => EvaluateStatement(s));
        private void EvaluateVariableDeclaration(BoundVariableDeclaration statement)
            => _lastValue = _variables[statement.Variable] = EvaluateExpression(statement.Initializer);
        private void EvaluateIfStatement(BoundIfStatement statement)
        {
            var condition = (bool)EvaluateExpression(statement.Condition);
            if (condition)
                EvaluateStatement(statement.ThenStatement);
            else if (statement.ElseStatement != null)
                EvaluateStatement(statement.ElseStatement);
        }
        private void EvaluateWhileStatement(BoundWhileStatement statement)
        {
            while ((bool)EvaluateExpression(statement.Condition))
                EvaluateStatement(statement.WhileStatement);
        }
        private void EvaluateForStatement(BoundForStatement statement)
        {
            EvaluateStatement(statement.DeclarationStatement);
            while ((bool)EvaluateExpression(statement.Condition))
            {
                EvaluateStatement(statement.ForStatement);
                EvaluateExpression(statement.Increment);
            }
        }
        private void EvaluateExpressionStatement(BoundExpressionStatement statement) => _lastValue = EvaluateExpression(statement.Expression);
        private object EvaluateExpression(BoundExpression node) => node switch {
            BoundLiteralExpression l => EvaluateLiteralExpression(l),
            BoundVariableExpression v => EvaluateVariableExpression(v),
            BoundAssigmentExpression a => EvaluateAssigmentExpression(a),
            BoundUnaryExpression u => EvaluateUnaryExpression(u),
            BoundBinaryExpression b => EvaluateBinaryExpression(b),
            _ => throw new Exception($"Unexpected node {node.Kind}"),
        };
        private static object EvaluateLiteralExpression(BoundLiteralExpression l) => l.Value;
        private object EvaluateVariableExpression(BoundVariableExpression v) => _variables[v.Variable];
        private object EvaluateAssigmentExpression(BoundAssigmentExpression a) => _variables[a.Variable] = EvaluateExpression(a.Expression);
        private object EvaluateUnaryExpression(BoundUnaryExpression u) => u.Op.Kind switch {
            BoundUnaryOperatorKind.Indentity => (int)EvaluateExpression(u.Operand),
            BoundUnaryOperatorKind.Negation => -(int)EvaluateExpression(u.Operand),
            BoundUnaryOperatorKind.LogicalNegation => !(bool)EvaluateExpression(u.Operand),
            BoundUnaryOperatorKind.OnesComplement => ~(int)EvaluateExpression(u.Operand),
            _ => throw new Exception($"Unexpected unary operator {u.Op}"),
        };
        private object EvaluateBinaryExpression(BoundBinaryExpression b) => b.Op.Kind switch {
            BoundBinaryOperatorKind.Addition => (int)EvaluateExpression(b.Left) + (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Subtraction => (int)EvaluateExpression(b.Left) - (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Multiplication => (int)EvaluateExpression(b.Left) * (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Division => (int)EvaluateExpression(b.Left) / (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Modulo => (int)EvaluateExpression(b.Left) % (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Less => (int)EvaluateExpression(b.Left) < (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.LessOrEquals => (int)EvaluateExpression(b.Left) <= (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Greater => (int)EvaluateExpression(b.Left) > (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.GreaterOrEquals => (int)EvaluateExpression(b.Left) >= (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.LogicalAnd => (bool)EvaluateExpression(b.Left) && (bool)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.LogicalOr => (bool)EvaluateExpression(b.Left) || (bool)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.Equals => Equals(EvaluateExpression(b.Left), EvaluateExpression(b.Right)),
            BoundBinaryOperatorKind.Diferent => !Equals(EvaluateExpression(b.Left), EvaluateExpression(b.Right)),
            BoundBinaryOperatorKind.BitwiseXor => b.Type != typeof(int) ? (bool)EvaluateExpression(b.Left) ^ (bool)EvaluateExpression(b.Right)
                                                                        : (int)EvaluateExpression(b.Left) ^ (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.BitwiseOr => b.Type != typeof(int) ? (bool)EvaluateExpression(b.Left) | (bool)EvaluateExpression(b.Right)
                                                                       : (int)EvaluateExpression(b.Left) | (int)EvaluateExpression(b.Right),
            BoundBinaryOperatorKind.BitwiseAnd => b.Type != typeof(int) ? (bool)EvaluateExpression(b.Left) & (bool)EvaluateExpression(b.Right)
                                                                        : (int)EvaluateExpression(b.Left) & (int)EvaluateExpression(b.Right),
            _ => throw new Exception($"Unexpected binary operator {b.Op}"),
        };
    }
}