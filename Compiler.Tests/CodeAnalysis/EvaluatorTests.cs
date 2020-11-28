using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Syntax;
using System.Collections.Generic;
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
        public void Evalutor_Works(string text, object value)
        {
            EvaluationResult result = new Compilation(SyntaxTree.Parse(text)).Evaluate(new Dictionary<VariableSymbol, object>());
            Assert.Empty(result.Diagnostics);
            Assert.Equal(value, result.Value);
        }
    }
}