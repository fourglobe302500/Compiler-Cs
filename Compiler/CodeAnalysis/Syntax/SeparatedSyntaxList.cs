using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators( );
    }

    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
        where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> _separatorsAndNodes;

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> separatorsAndNodes) => _separatorsAndNodes = separatorsAndNodes;
        public int Count => (_separatorsAndNodes.Length + 1) / 2;
        public T this[int index] => (T)_separatorsAndNodes[index * 2];

        public SyntaxToken GetSeparator(int index) => index == Count - 1 ? null : (SyntaxToken)_separatorsAndNodes[1 + (index * 2)];

        public override ImmutableArray<SyntaxNode> GetWithSeparators( ) => _separatorsAndNodes;

        public IEnumerator<T> GetEnumerator( )
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator( ) => GetEnumerator();
    }
}