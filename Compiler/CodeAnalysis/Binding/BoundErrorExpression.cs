﻿
using Compiler.CodeAnalysis.Symbols;

namespace Compiler.CodeAnalysis.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override TypeSymbol Type => TypeSymbol.Error;
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    }
}