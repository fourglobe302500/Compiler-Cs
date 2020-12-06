using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Symbols;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;

namespace Compiler.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator( ) => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator( ) => GetEnumerator();

        public void AddRange(DiagnosticBag diagnostics)
          => _diagnostics.AddRange(diagnostics._diagnostics);

        private void Report(TextSpan span, string message)
          => _diagnostics.Add(new Diagnostic(span, message));

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
          => Report(span, $"The number{text} isnt't a valis {type}.");

        public void ReportInvalidCharacter(int position, char character)
          => Report(new TextSpan(position, 1), $"Invalid Token '{character}'.");

        public void ReportInvalidStringFormat(TextSpan span)
            => Report(span, "Invalid string format: mult-line string or missing '\"'.");

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
          => Report(span, $"Unexpected token <{actualKind}>, expected <{expectedKind}>.");

        public void ReportUndefinedUnaryOperator(TextSpan span, string text, TypeSymbol type)
          => Report(span, $"Unary operator '{text}' is not defined for type '{type}'.");

        public void ReportUndefinedBinaryOperator(TextSpan span, string text, TypeSymbol leftType, TypeSymbol rightType)
          => Report(span, $"Binary operator '{text}' is not defined for types '{leftType}' and '{rightType}'.");

        public void ReportUndefinedName(TextSpan span, string name)
            => Report(span, $"Variable '{name}' doesn't exist.");

        public void ReportUndefinedFunction(TextSpan span, string text)
            => Report(span, $"Undefined function '{text}'.");

        public void ReportUndefinedFunctionForNumberOfArguments(TextSpan span, string text, int count)
            => Report(span, $"No overload for function {text} with {count} parameters.");

        public void ReportInvalidSetOfArgs(TextSpan span, string text, ImmutableArray<BoundExpression> arguments)
        {
            var message = $"No overload for function {text} that matchs set of parameters: [";
            var first = true;
            foreach (var arg in arguments)
            {
                if (first)
                    first = false;
                else
                    message += ", ";
                message += arg.Type;
            }
            Report(span, message + "].");
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
            => Report(span, $"Cannot convert type '{fromType}' to '{toType}'.");

        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
            => Report(span, $"Variable '{name}' is already declared.");

        public void ReportCannotAssign(TextSpan span, string name)
            => Report(span, $"Variable '{name}' is read-only and cannot be reassigned.");

        public void ReportExpressionMustHaveValue(TextSpan span)
            => Report(span, "Type must have a valid type.");
    }
}