using Compiler.CodeAnalysis.Syntax;
using System;
using System.Linq;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(
            SyntaxKind syntaxKind,
            BoundBinaryOperatorKind kind,
            Type type)
                : this(syntaxKind, kind, type, type, type)
        {
        }

        private BoundBinaryOperator(
            SyntaxKind syntaxKind,
            BoundBinaryOperatorKind kind,
            Type operandType,
            Type resultType)
                : this(syntaxKind, kind, operandType, operandType, resultType)
        {
        }

        private BoundBinaryOperator(
            SyntaxKind syntaxKind,
            BoundBinaryOperatorKind kind,
            Type leftType,
            Type rightType,
            Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; private set; }
        public Type RightType { get; }
        public Type Type { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(
                SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,
                typeof(int)),
            new BoundBinaryOperator(
                SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,
                typeof(int)),
            new BoundBinaryOperator(
                SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,
                typeof(int)),
            new BoundBinaryOperator(
                SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,
                typeof(int)),
            new BoundBinaryOperator(
                SyntaxKind.PercentToken,BoundBinaryOperatorKind.Modulo,
                typeof(int)),
            new BoundBinaryOperator(
                SyntaxKind.HatToken,BoundBinaryOperatorKind.Power,
                typeof(int)),
            new BoundBinaryOperator(
                SyntaxKind.DoubleEqualsToken,BoundBinaryOperatorKind.Equals,
                typeof(int),typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.NotEqualsToken,BoundBinaryOperatorKind.Diferent,
                typeof(int),typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.LessOrEqualsThenToken,BoundBinaryOperatorKind.LessOrEquals,
                typeof(int),typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.LessThenToken,BoundBinaryOperatorKind.Less,
                typeof(int),typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.GreaterOrEqualsThenToken,BoundBinaryOperatorKind.GreaterOrEquals,
                typeof(int),typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.GreaterThenToken,BoundBinaryOperatorKind.Greater,
                typeof(int),typeof(bool)),

            new BoundBinaryOperator(
                SyntaxKind.LogicalAndToken,BoundBinaryOperatorKind.LogicalAnd,
                typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.LogicalOrToken,BoundBinaryOperatorKind.LogicalOr,
                typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.DoubleEqualsToken,BoundBinaryOperatorKind.Equals,
                typeof(bool),typeof(bool)),
            new BoundBinaryOperator(
                SyntaxKind.NotEqualsToken,BoundBinaryOperatorKind.Diferent,
                typeof(bool),typeof(bool)),
        };

        public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
        {
            foreach (BoundBinaryOperator op in _operators.Where(op => op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType))
            {
                return op;
            }
            return null;
        }
    }
}