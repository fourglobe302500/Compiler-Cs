using System;
using System.Collections.Generic;
using Compiler.CodeAnalysis.Syntax;

namespace Compiler.CodeAnalysis.Binding
{
  internal sealed class Binder
  {
    private readonly List<string> _diagnostics = new List<string>();
    public List<string> Diagnostics => _diagnostics;

    public BoundExpression BindExpression(ExpressionSyntax syntax) 
    {
      if (syntax.Kind == SyntaxKind.LiteralExpression) 
        return  BindLiteralExpression((LiteralExpressionSyntax)syntax);
      else if (syntax.Kind == SyntaxKind.UnaryExpression) 
        return  BindUnaryExpression((UnaryExpressionSyntax)syntax);
      else if (syntax.Kind == SyntaxKind.BinaryExpression) 
        return  BindBinaryExpression((BinaryExpressionSyntax)syntax);
      else if (syntax.Kind == SyntaxKind.ParenthesizedExpression) 
        return  BindExpression(((ParenthesizedExpressionSyntax)syntax).Expression);
      else 
        throw new Exception($"Unaexpected syntax {syntax.Kind}");
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax) 
      => new BoundLiteralExpression(syntax.Value ?? 0);

    private BoundExpression BindUnaryExpression(
      UnaryExpressionSyntax syntax)
    {
      var boundOperand = BindExpression(syntax.Operand);
      var boundOperator = BoundUnaryOperator.Bind(
        syntax.OperatorToken.Kind, 
        boundOperand.Type); 
      if (boundOperator == null)
      {
        _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not defined for type {boundOperand.Type}");
        return boundOperand;
      }
      return new BoundUnaryExpression(boundOperator, boundOperand);
    }

    private BoundExpression BindBinaryExpression(
      BinaryExpressionSyntax syntax)
    {
      var boundLeft = BindExpression(syntax.Left);
      var boundRight = BindExpression(syntax.Right);
      var boundOperator = BoundBinaryOperator.Bind(
        syntax.OperatorToken.Kind,
        boundLeft.Type, 
        boundRight.Type);
      if (boundOperator == null)
      {
        _diagnostics.Add($"Binary operator '{syntax.OperatorToken.Text}' is not defined for types {boundLeft.Type} and {boundRight.Type}");
        return boundLeft;
      }
      return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
    }
  }
}