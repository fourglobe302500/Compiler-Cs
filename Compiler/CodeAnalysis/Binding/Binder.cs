using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Compiler.CodeAnalysis.Syntax;
namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private BoundScope _scope;
        public Binder(BoundScope parent) => _scope = new BoundScope(parent);
        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScopes(previous);
            var binder = new Binder(parentScope);
            var expression = binder.BindStatement(syntax.Statement);
            var variables = binder._scope.Variables;
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }
        private static BoundScope CreateParentScopes(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }
            BoundScope parent = null;

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                previous.Variables.ToImmutableList().ForEach(v => scope.TryDeclare(v));
                parent = scope;
            }

            return parent;
        }
        public DiagnosticBag Diagnostics => _diagnostics;
        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            return syntax.Kind switch {
                SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax)syntax),
                SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclarationSyntax)syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }
        private BoundBlockStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);
            syntax.Statements.ToImmutableList().ForEach(s => statements.Add(BindStatement(s)));
            _scope = _scope.Parent;
            return new BoundBlockStatement(statements.ToImmutable());
        }
        private BoundVariableDeclaration BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.DefKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable = new VariableSymbol(name, isReadOnly, initializer.Type);

            if (!_scope.TryDeclare(variable))
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

            return new BoundVariableDeclaration(variable, initializer);
        }
        private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
            => new BoundExpressionStatement(BindExpression(syntax.Expression));
        private BoundExpression BindExpression(ExpressionSyntax syntax) => syntax.Kind switch {
            SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax)syntax),
            SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax),
            SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax)syntax),
            SyntaxKind.AssigmentExpression => BindAssigmentxpression((AssigmentExpressionSyntax)syntax),
            SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax)syntax),
            SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax)syntax),
            _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
        };
        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
          => new BoundLiteralExpression(syntax.Value ?? 0);
        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;

            if (!_scope.TryLookup(name, out var variable))
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

            if (!_scope.TryLookup(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

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