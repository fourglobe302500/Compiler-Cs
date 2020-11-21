using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Compiler.CodeAnalysis.Text;

namespace Compiler.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public virtual TextSpan Span => TextSpan.FromBounds(GetChildren().First().Span.Start, GetChildren().Last().Span.End);

        public IEnumerable<SyntaxNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                    yield return (SyntaxNode)property.GetValue(this);
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                    foreach (var child in (IEnumerable<SyntaxNode>)property.GetValue(this))
                        yield return child;
            }
        }

        public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);

        private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool last = true)
        {
            writer.Write(value: $"{indent}{(last ? "└──" : "├──")}{node.Kind}");
            if (node is SyntaxToken t && t.Value != null)
                writer.Write($": {t.Value}");
            indent += last ? "   " : "│  ";
            writer.WriteLine();
            foreach (var child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == node.GetChildren().LastOrDefault());
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);

                return writer.ToString();
            }
        } 
    }
}   