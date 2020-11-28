using System;
using System.Linq;

using Compiler.CodeAnalysis.Syntax;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType)
                : this(syntaxKind, kind, operandType, operandType)
        {
        }
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            OperandType = operandType;
            Type = resultType;
        }
        public SyntaxKind SyntaxKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public Type OperandType { get; }
        public Type Type { get; }
        private static readonly BoundUnaryOperator[] _operators =
        {
            new BoundUnaryOperator(
                SyntaxKind.ExclamationToken,
                BoundUnaryOperatorKind.LogicalNegation,
                typeof(bool)),

            new BoundUnaryOperator(
                SyntaxKind.MinusToken,
                BoundUnaryOperatorKind.Negation,
                typeof(int)),

            new BoundUnaryOperator(
                SyntaxKind.PlusToken,
                BoundUnaryOperatorKind.Indentity,
                typeof(int)),
        };
        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, Type operandType)
        {
            foreach (BoundUnaryOperator op in _operators.Where(op => op.SyntaxKind == syntaxKind && op.OperandType == operandType))
                return op;
            return null;
        }
    }
}