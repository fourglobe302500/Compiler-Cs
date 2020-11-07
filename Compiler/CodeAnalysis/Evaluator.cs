using System;
using Compiler.CodeAnalysis.Binding;

namespace Compiler.CodeAnalysis
{
  internal sealed class Evaluator
  {
    private readonly BoundExpression _root;

    public Evaluator(BoundExpression root) => _root = root;

    public object Evaluate => EvaluateExpression(_root);

    private object EvaluateExpression(BoundExpression node) 
    {
      if (node is BoundLiteralExpression l) 
        return l.Value;
      else if (node is BoundUnaryExpression u) 
        switch(u.Op.Kind)
        {
          case BoundUnaryOperatorKind.Indentity: 
            return (int)EvaluateExpression(u.Operand);
          case BoundUnaryOperatorKind.Negation:
            return -(int)EvaluateExpression(u.Operand);
          case BoundUnaryOperatorKind.LogicalNegation:
          return !(bool)EvaluateExpression(u.Operand);
          default: throw new Exception(
            $"Unexpected unary operator {u.Op}");
        }
      else if (node is BoundBinaryExpression b)
      {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
        switch(b.Op.Kind)
        {
          case BoundBinaryOperatorKind.Addition: 
            return (int)left + (int)right;
          case BoundBinaryOperatorKind.Subtraction: 
            return (int)left - (int)right;
          case BoundBinaryOperatorKind.Multiplication: 
            return (int)left * (int)right;
          case BoundBinaryOperatorKind.Division: 
            return (int)left / (int)right;
          case BoundBinaryOperatorKind.Modulo: 
            return (int)left % (int)right;
          case BoundBinaryOperatorKind.Power: 
            return (int)Math.Pow((int)left, (int)right);
          case BoundBinaryOperatorKind.Less: 
            return  (int)left <  (int)right;
          case BoundBinaryOperatorKind.LessOrEquals: 
            return  (int)left <=  (int)right;
          case BoundBinaryOperatorKind.Greater: 
            return  (int)left >  (int)right;
          case BoundBinaryOperatorKind.GreaterOrEquals: 
            return  (int)left >=  (int)right;
          case BoundBinaryOperatorKind.LogicalAnd: 
            return (bool)left && (bool)right;
          case BoundBinaryOperatorKind.LogicalOr: 
            return (bool)left || (bool)right;
          case BoundBinaryOperatorKind.Equals: 
            return Equals(left, right);
          case BoundBinaryOperatorKind.Diferent: 
            return !Equals(left, right);
          default: throw new Exception(
            $"Unexpected binary operator {b.Op}");
        };
      }
      else throw new Exception($"Unexpected node {node.Kind}");
    }
  }
}