using System.Collections.Generic;
using Compiler.CodeAnalysis.Syntax;
using Xunit;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
        {
            var op1Prec = SyntaxFacts.GetBinaryOperatorPrecedence(op1);
            var op2Prec = SyntaxFacts.GetBinaryOperatorPrecedence(op2);
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = SyntaxTree.Parse(text).Root;
            
            if (op1Prec >= op2Prec)
            {
                //      op2
                //     /  \
                //   op1  c
                //  /  \
                // a    b
                using (var e = new AssertingEnumerator(expression))
                {
                    //op2
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //op1
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //a & operator1 & b
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    e.AssertToken(op1, op1Text);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                    //operator2
                    e.AssertToken(op2, op2Text);
                    //c
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
            else
            {
                //   op1
                //  /  \
                // a   op2
                //    /  \
                //   b    c
                using (var e = new AssertingEnumerator(expression))
                {
                    //op1
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //a
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    //operator1
                    e.AssertToken(op1, op1Text);
                    //op2
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //b & operator 2 & c
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                    e.AssertToken(op2, op2Text);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperatorKinds())
                foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
                    yield return new object[] { op1, op2 };
        }
    }
}
