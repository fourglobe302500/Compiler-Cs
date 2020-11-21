using System;
using System.Collections.Generic;
using Compiler.CodeAnalysis.Binding;

namespace Compiler.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly BoundExpression _root;

        public Evaluator(
            BoundExpression root,
            System.Collections.Generic.Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate => EvaluateExpression(_root);

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node)
            {
                case BoundLiteralExpression l:
                    return EvaluateLiteralExpression(l);
                case BoundVariableExpression v:
                    return EvaluateVariableExpression(v);
                case BoundAssigmentExpression a:
                    return EvaluateAssigmentExpression(a);
                case BoundUnaryExpression u:
                    return EvaluateUnaryExpression(u);
                case BoundBinaryExpression b:
                    return EvaluateBinaryExpression(b);
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression l) => l.Value;

        private object EvaluateVariableExpression(BoundVariableExpression v) => _variables[v.Variable];

        private object EvaluateAssigmentExpression(BoundAssigmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            _variables[a.Variable] = value;
            return value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);
            switch (u.Op.Kind)
            {
                case BoundUnaryOperatorKind.Indentity:
                    return (int)operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                default:
                    throw new Exception(
                    $"Unexpected unary operator {u.Op}");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (int)EvaluateExpression(b.Left) + (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Subtraction:
                    return (int)EvaluateExpression(b.Left) - (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)EvaluateExpression(b.Left) * (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Division:
                    return (int)EvaluateExpression(b.Left) / (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Modulo:
                    return (int)EvaluateExpression(b.Left) % (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Power:
                    return (int)Math.Pow((int)EvaluateExpression(b.Left), (int)EvaluateExpression(b.Right));
                case BoundBinaryOperatorKind.Less:
                    return (int)EvaluateExpression(b.Left) < (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.LessOrEquals:
                    return (int)EvaluateExpression(b.Left) <= (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Greater:
                    return (int)EvaluateExpression(b.Left) > (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return (int)EvaluateExpression(b.Left) >= (int)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)EvaluateExpression(b.Left) && (bool)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)EvaluateExpression(b.Left) || (bool)EvaluateExpression(b.Right);
                case BoundBinaryOperatorKind.Equals:
                    return Equals(EvaluateExpression(b.Left), EvaluateExpression(b.Right));
                case BoundBinaryOperatorKind.Diferent:
                    return !Equals(EvaluateExpression(b.Left), EvaluateExpression(b.Right));
                default:
                    throw new Exception(
                    $"Unexpected binary operator {b.Op}");
            }
        }
    }
}