using System;
using System.Collections.Generic;
using Compiler.CodeAnalysis.Binding;

namespace Compiler.CodeAnalysis
{
  internal sealed class Evaluator
  {
    private readonly Dictionary<string, object> _variables;
    private readonly BoundExpression _root;

    public Evaluator(
      BoundExpression root,
      System.Collections.Generic.Dictionary<string, object> variables)
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
          return l.Value;
        case BoundVariableExpression v:
          return _variables[v.Name];
        case BoundAssigmentExpression a:
          var value = EvaluateExpression(a.Expression);
          _variables[a.Name] = value;
          return value;
        case BoundUnaryExpression u:
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
        case BoundBinaryExpression b:
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
        default:
          throw new Exception($"Unexpected node {node.Kind}");
      }
    }
  }
}