using Compiler.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        public Binder(Dictionary<VariableSymbol, object> variables)
        {
            _variables = variables;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        public BoundExpression BindExpression(ExpressionSyntax syntax) => syntax.Kind switch
        {
            SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax)syntax),
            SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax),
            SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax)syntax),
            SyntaxKind.AssigmentExpression => BindAssigmentxpression((AssigmentExpressionSyntax)syntax),
            SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax)syntax),
            SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax)syntax),
            _ => throw new Exception($"Unaexpected syntax {syntax.Kind}"),
        };

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
          => new BoundLiteralExpression(syntax.Value ?? 0);

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;
            VariableSymbol variable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (variable == null)
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssigmentxpression(AssigmentExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;
            BoundExpression boundExpression = BindExpression(syntax.Expression);

            VariableSymbol existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);
            if (existingVariable != null)
                _variables.Remove(existingVariable);
            VariableSymbol variable = new VariableSymbol(name, boundExpression.Type);
            _variables[variable] = null;

            return new BoundAssigmentExpression(variable, boundExpression);
        }

        private BoundExpression BindUnaryExpression(
          UnaryExpressionSyntax syntax)
        {
            BoundExpression boundOperand = BindExpression(syntax.Operand);
            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(
              syntax.OperatorToken.Kind,
              boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(
                  syntax.OperatorToken.Span,
                  syntax.OperatorToken.Text,
                  boundOperand.Type);
                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(
          BinaryExpressionSyntax syntax)
        {
            BoundExpression boundLeft = BindExpression(syntax.Left);
            BoundExpression boundRight = BindExpression(syntax.Right);
            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(
              syntax.OperatorToken.Kind,
              boundLeft.Type,
              boundRight.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(
                  syntax.OperatorToken.Span,
                  syntax.OperatorToken.Text,
                  boundLeft.Type,
                  boundRight.Type);
                return boundLeft;
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax) => BindExpression(syntax.Expression);
    }
}