using System;
using System.Collections.Generic;

using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;

using Xunit;
namespace Compiler.Tests.CodeAnalysis
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        [InlineData("1 + 2", 3)]
        [InlineData("9 - 12", -3)]
        [InlineData("3 * 6", 18)]
        [InlineData("4 / 2", 2)]
        [InlineData("10 % 3", 1)]
        [InlineData("2 ^ 3", 8)]
        [InlineData("(10)", 10)]
        [InlineData("1 == 12", false)]
        [InlineData("1 == 1", true)]
        [InlineData("2 != 11", true)]
        [InlineData("11 != 11", false)]
        [InlineData("5 >  2", true)]
        [InlineData("5 >  8", false)]
        [InlineData("3 <  10", true)]
        [InlineData("3 <  -4", false)]
        [InlineData("6 >= 6", true)]
        [InlineData("6 >= 7", false)]
        [InlineData("4 <= 9", true)]
        [InlineData("4 <= -9", false)]
        [InlineData("true == true", true)]
        [InlineData("true == false", false)]
        [InlineData("false != false", false)]
        [InlineData("true != false", true)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("true && false", false)]
        [InlineData("false || false", false)]
        [InlineData("{ var a = 0 (a = 10) * 2}", 20)]
        [InlineData("{ var a = 0 if a == 0 a = 10 a}", 10)]
        [InlineData("{ var a = 20 if (a == 0) a = 10 else a = 9}", 9)]
        [InlineData("{ var a = 50 if (a == 50) {a = a - 10 if (a == 20) a = a * 2 else a = a / 2}}", 20)]
        [InlineData("{var x = 0 while (x < 10) x = x + 1 x }", 10)]
        [InlineData("{var i = 1 for var x = 0 x < 4 x = x + 1 {i = i * 2}}", 16)]
        public void Evalutor_Works(string text, object value)
        {
            EvaluationResult result = new Compilation(SyntaxTree.Parse(text)).Evaluate(new Dictionary<VariableSymbol, object>());
            Assert.Empty(result.Diagnostics);
            Assert.Equal(value, result.Value);
        }
        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration( )
        {
            var text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
            ";

            var diagnostics = @"
                Variable 'x' is already declared.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_Name_Reports_Undefined( )
        {
            var text = @"[x] * 10";

            var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_Assigned_Reports_Undefined( )
        {
            var text = @"[x] = 10";

            var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_Assigned_Reports_Reassigment( )
        {
            var text = @"
                {
                    def x = 10
                    x [=] 100
                }
            ";

            var diagnostics = @"
                Variable 'x' is read-only and cannot be reassigned.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_Assigned_Reports_Conversion( )
        {
            var text = @"
                {
                    var x = 10
                    x = [true]
                }
            ";

            var diagnostics = @"
                Cannot convert type 'System.Boolean' to 'System.Int32'.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_Unary_Reports_Undefined( )
        {
            var text = @"[+]true";

            var diagnostics = @"
                Unary operator '+' is not defined for type 'System.Boolean'.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_ifStatement_Reports_Conversion( )
        {
            var text = @"
                {
                    var x = 0
                    if [10]
                        x = 10
                }
            ";

            var diagnostics = @"
                Cannot convert type 'System.Int32' to 'System.Boolean'.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_WhileStatement_Reports_Conversion( )
        {
            var text = @"
                {
                    var x = 0
                    while [10]
                        x = 10
                }
            ";

            var diagnostics = @"
                Cannot convert type 'System.Int32' to 'System.Boolean'.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaluator_Bynary_Reports_( )
        {
            var text = @"10 [+] false";

            var diagnostics = @"
                Binary operator '+' is not defined for types 'System.Int32' and 'System.Boolean'.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);
            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
                throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");
            Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);
            for (var i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Diagnostics[i].Span;

                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = result.Diagnostics[i].Message;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Equal(expectedSpan.ToString(), actualSpan.ToString());
            }
        }
    }
}