using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Symbols;

namespace Compiler.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly BoundBlockStatement _root;
        private object _lastValue;
        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }
        public object Evaluate( )
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (var i = 0; i < _root.Statements.Length; i++)
                if (_root.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i);
            var index = 0;
            while (index < _root.Statements.Length)
            {
                var s = _root.Statements[index];
                switch (s)
                {
                    case BoundVariableDeclaration v:
                        EvaluateVariableDeclaration(v);
                        break;
                    case BoundExpressionStatement e:
                        EvaluateExpressionStatement(e);
                        break;
                    case BoundGotoStatement g:
                        index = labelToIndex[g.Label];
                        break;
                    case BoundConditionalGotoStatement c:
                        if ((bool)EvaluateExpression(c.Condition) == c.JumpIfTrue) index = labelToIndex[c.Label];
                        break;
                    case BoundLabelStatement:
                        break;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
                index++;
            }
            return _lastValue;
        }
        private void EvaluateVariableDeclaration(BoundVariableDeclaration statement)
        {
            var value = EvaluateExpression(statement.Initializer);
            _variables[statement.Variable] = value;
            _lastValue = value;
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