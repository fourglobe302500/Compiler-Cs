using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Compiler.CodeAnalysis.Symbols;
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
            Binder binder = new Binder(CreateParentScopes(previous));
            BoundStatement expression = binder.BindStatement(syntax.Statement);
            return new BoundGlobalScope(
                previous,
                previous == null ?
                    binder.Diagnostics.ToImmutableArray() :
                    binder.Diagnostics.ToImmutableArray()
                                      .InsertRange(0, previous.Diagnostics),
                binder._scope.Variables,
                expression);
        }

        private static BoundScope CreateParentScopes(BoundGlobalScope previous)
        {
            Stack<BoundGlobalScope> stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }
            BoundScope parent = null;
            while (stack.Count > 0)
            {
                BoundScope scope = new BoundScope(parent);
                stack.Pop().Variables.ToImmutableList().ForEach(v => scope.TryDeclare(v));
                parent = scope;
            }
            return parent;
        }
        public DiagnosticBag Diagnostics => _diagnostics;

        private BoundStatement BindStatement(StatementSyntax syntax) => syntax.Kind switch {
            SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax)syntax),
            SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclarationSyntax)syntax),
            SyntaxKind.IfStatement => BindIfStatement((IfStatementSyntax)syntax),
            SyntaxKind.WhileStatement => BindWhileStatement((WhileStatementSyntax)syntax),
            SyntaxKind.ForStatement => BindForStatement((ForStatementSyntax)syntax),
            SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax)syntax),
            _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
        };

        private BoundBlockStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            ImmutableArray<BoundStatement>.Builder statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);
            syntax.Statements.ToImmutableList().ForEach(s => statements.Add(BindStatement(s)));
            _scope = _scope.Parent;
            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundVariableDeclaration BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            BoundExpression initializer = BindExpression(syntax.Initializer);
            VariableSymbol variable = new VariableSymbol(syntax.Identifier.Text ?? "?", syntax.Keyword.Kind == SyntaxKind.DefKeyword, initializer.Type);
            if (!syntax.Identifier.IsMissing && !_scope.TryDeclare(variable))
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, syntax.Identifier.Text);
            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundIfStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.Code);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundWhileStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var whileStatement = BindStatement(syntax.WhileStatement);
            return new BoundWhileStatement(condition, whileStatement);
        }

        private BoundForStatement BindForStatement(ForStatementSyntax syntax)
        {
            _scope = new BoundScope(_scope);
            var declarationStatement = BindStatement(syntax.DeclaritionStatement);
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var incrementExpression = BindExpression(syntax.IncrementExpression);
            var forStatement = BindStatement(syntax.ForStatement);
            _scope = _scope.Parent;
            return new BoundForStatement(declarationStatement, condition, incrementExpression, forStatement);
        }

        private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
            => new BoundExpressionStatement(BindExpression(syntax.Expression));

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol returnType)
        {
            var result = BindExpression(syntax);
            if (returnType != TypeSymbol.Error && result.Type != TypeSymbol.Error && result.Type != returnType)
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, returnType);
            return result;
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax) => syntax.Kind switch {
            SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax)syntax),
            SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax),
            SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax)syntax),
            SyntaxKind.AssigmentExpression => BindAssigmentxpression((AssigmentExpressionSyntax)syntax),
            SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax)syntax),
            SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax)syntax),
            _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
        };

        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
          => new BoundLiteralExpression(syntax.Value ?? 0);

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            string name = syntax.IdentifierToken.Text;
            if (syntax.IdentifierToken.IsMissing)
                return new BoundErrorExpression();
            if (!_scope.TryLookup(name, out VariableSymbol variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, syntax.IdentifierToken.Text);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssigmentxpression(AssigmentExpressionSyntax syntax)
        {
            BoundExpression boundExpression = BindExpression(syntax.Expression);
            if (!_scope.TryLookup(syntax.IdentifierToken.Text, out VariableSymbol variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, syntax.IdentifierToken.Text);
                return boundExpression;
            }
            if (variable.IsReadOnly)
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, syntax.IdentifierToken.Text);
            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }
            return new BoundAssigmentExpression(variable, boundExpression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            BoundExpression boundOperand = BindExpression(syntax.Operand);
            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperand.Kind == BoundNodeKind.ErrorExpression)
                return new BoundErrorExpression();
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            BoundExpression boundLeft = BindExpression(syntax.Left);
            BoundExpression boundRight = BindExpression(syntax.Right);
            if (boundLeft.Kind == BoundNodeKind.ErrorExpression || boundRight.Kind == BoundNodeKind.ErrorExpression)
                return new BoundErrorExpression();
            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax) => BindExpression(syntax.Expression);
    }
}