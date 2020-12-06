using System.Linq;

using Compiler.CodeAnalysis.Symbols;
using Compiler.CodeAnalysis.Syntax;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type)
                : this(syntaxKind, kind, type, type, type)
        {
        }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
                : this(syntaxKind, kind, operandType, operandType, resultType)
        {
        }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }
        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; private set; }
        public TypeSymbol RightType { get; }
        public TypeSymbol Type { get; }
        private static readonly BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.PercentToken,BoundBinaryOperatorKind.Modulo,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.HatToken,BoundBinaryOperatorKind.BitwiseXor,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.PipeToken,BoundBinaryOperatorKind.BitwiseOr,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.AmpersandToken,BoundBinaryOperatorKind.BitwiseAnd,TypeSymbol.Int),

            new BoundBinaryOperator(SyntaxKind.DoubleEqualsToken,BoundBinaryOperatorKind.Equals,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.NotEqualsToken,BoundBinaryOperatorKind.Diferent,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsThenToken,BoundBinaryOperatorKind.LessOrEquals,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessThenToken,BoundBinaryOperatorKind.Less,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsThenToken,BoundBinaryOperatorKind.GreaterOrEquals,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterThenToken,BoundBinaryOperatorKind.Greater,TypeSymbol.Int,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.AmpersandToken,BoundBinaryOperatorKind.BitwiseAnd,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken,BoundBinaryOperatorKind.LogicalAnd,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PipeToken,BoundBinaryOperatorKind.BitwiseOr,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken,BoundBinaryOperatorKind.LogicalOr,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.HatToken,BoundBinaryOperatorKind.BitwiseXor,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.DoubleEqualsToken,BoundBinaryOperatorKind.Equals,TypeSymbol.Bool,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.NotEqualsToken,BoundBinaryOperatorKind.Diferent,TypeSymbol.Bool,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition, TypeSymbol.String),
        };

        public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (BoundBinaryOperator op in _operators.Where(op => op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType))
                return op;
            return null;
        }
    }
}