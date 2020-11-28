using System.Collections.Generic;
using System.Linq;

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
            int op1Prec = SyntaxFacts.GetBinaryOperatorPrecedence(op1);
            int op2Prec = SyntaxFacts.GetBinaryOperatorPrecedence(op2);
            string op1Text = SyntaxFacts.GetText(op1);
            string op2Text = SyntaxFacts.GetText(op2);
            string text = $"a {op1Text} b {op2Text} c";
            ExpressionSyntax expression = ParseExpression(text);
            if (op1Prec >= op2Prec)
                //      op2
                //     /  \
                //   op1  c
                //  /  \
                // a    b
                using (AssertingEnumerator e = new AssertingEnumerator(expression))
                {
                    //op2
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //op1
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //a
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    //operator1
                    e.AssertToken(op1, op1Text);
                    //b
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                    //operator2
                    e.AssertToken(op2, op2Text);
                    //c
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            else
                //   op1
                //  /  \
                // a   op2
                //    /  \
                //   b    c
                using (AssertingEnumerator e = new AssertingEnumerator(expression))
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
                    //b
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                    //operator2
                    e.AssertToken(op2, op2Text);
                    //c
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
        }
        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsPrecedences(
            SyntaxKind unaryKind,
            SyntaxKind binaryKind)
        {
            int unaryPrec = SyntaxFacts.GetUnaryOperatorPrecedence(unaryKind);
            int binaryPrec = SyntaxFacts.GetBinaryOperatorPrecedence(binaryKind);
            string unaryText = SyntaxFacts.GetText(unaryKind);
            string binaryText = SyntaxFacts.GetText(binaryKind);
            string text = $"{unaryText} a {binaryText} b";
            ExpressionSyntax expression = ParseExpression(text);

            if (unaryPrec >= binaryPrec)
                //     binary
                //     /    \
                // unary    b
                //  |
                //  a
                using (AssertingEnumerator e = new AssertingEnumerator(expression))
                {
                    //binary
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //unary
                    e.AssertNode(SyntaxKind.UnaryExpression);
                    //UnaryOperator
                    e.AssertToken(unaryKind, unaryText);
                    //a
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    //binaryOperator
                    e.AssertToken(binaryKind, binaryText);
                    //b
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            else
                //    unary
                //     |
                //  binary
                //  /    \
                // a     b
                using (AssertingEnumerator e = new AssertingEnumerator(expression))
                {
                    //unary
                    e.AssertNode(SyntaxKind.UnaryExpression);
                    //unaryOperator
                    e.AssertToken(unaryKind, unaryText);
                    //binary
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    //a
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    //binaryOperator
                    e.AssertToken(binaryKind, binaryText);
                    //b
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
        }
        public static IEnumerable<object[]> GetBinaryOperatorPairsData( ) => from op1 in SyntaxFacts.GetBinaryOperatorKinds()
                                                                             from op2 in SyntaxFacts.GetBinaryOperatorKinds()
                                                                             select new object[] { op1, op2 };
        public static IEnumerable<object[]> GetUnaryOperatorPairsData( ) => from unary in SyntaxFacts.GetUnaryOperatorKinds()
                                                                            from binary in SyntaxFacts.GetBinaryOperatorKinds()
                                                                            select new object[] { unary, binary };
        private static ExpressionSyntax ParseExpression(string text) => Assert.IsType<ExpressionStatementSyntax>(SyntaxTree.Parse(text).Root.Statement).Expression;
    }
}