using System;
using System.Collections;
using System.Collections.Generic;
using Compiler.CodeAnalysis.Syntax;

namespace Compiler.CodeAnalysis
{
  internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
  {
    private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void AddRange(DiagnosticBag diagnostics) 
      => _diagnostics.AddRange(diagnostics._diagnostics);

    private void Report(TextSpan span, string massage) 
      => _diagnostics.Add(new Diagnostic(span, massage));

    public void ReportInvalidNumber(TextSpan span, string text, Type type) 
      => Report(span, $"The number{text} isnt't a valis {type}.");

    public void ReportInvalidCharacter(int position, char character) 
      => Report(new TextSpan(position, 1), $"Invalid Token '{character}'.");

    public void ReportUnexpectedToken(
      TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind) 
      => Report(span, $"Unexpected token <{actualKind}>, expected <{expectedKind}>.");

    public void ReportUndefinedUnaryOperator(TextSpan span, string text, Type type)
      => Report(span, $"Unary operator '{text}' is not defined for type {type}");

    public void ReportUndefinedBinaryOperator(
      TextSpan span, string text, Type leftType, Type rightType)
      => Report(span, 
        $"Binary operator '{text}' is not defined for types {leftType} and {rightType}");

    public void ReportUndefinedName(TextSpan span, string name) 
      => Report(span, $"Variable {name} doesn't exist");
  }
}
