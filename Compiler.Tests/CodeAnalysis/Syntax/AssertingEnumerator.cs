using Compiler.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Compiler.Tests.CodeAnalysis.Syntax
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        private readonly IEnumerator<SyntaxNode> _enumerator;
        private bool _hasError;

        public AssertingEnumerator(SyntaxNode node) => _enumerator = Flatten(node).GetEnumerator();

        private bool MarkFailed() => !(_hasError = true);

        public void Dispose()
        {
            if (!_hasError)
                Assert.False(_enumerator.MoveNext());
            _enumerator.Dispose();
        }

        private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
        {
            Stack<SyntaxNode> stack = new Stack<SyntaxNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                SyntaxNode n = stack.Pop();
                yield return n;

                foreach (SyntaxNode child in n.GetChildren().Reverse())
                    stack.Push(child);
            }
        }

        public void AssertNode(SyntaxKind kind)
        {
            try
            {
                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                Assert.IsNotType<SyntaxToken>(_enumerator.Current);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertToken(SyntaxKind kind, string text)
        {
            try
            {
                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                SyntaxToken token = Assert.IsType<SyntaxToken>(_enumerator.Current);
                Assert.Equal(text, token.Text);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }
    }
}