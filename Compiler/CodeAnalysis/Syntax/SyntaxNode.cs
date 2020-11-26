using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Compiler.CodeAnalysis.Text;

namespace Compiler.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public virtual TextSpan Span => TextSpan.FromBounds(GetChildren().First().Span.Start, GetChildren().Last().Span.End);

        public IEnumerable<SyntaxNode> GetChildren ( )
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                    yield return (SyntaxNode)property.GetValue(this);
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    foreach (SyntaxNode child in (IEnumerable<SyntaxNode>)property.GetValue(this))
                        yield return child;
                }
            }
        }

        public void WriteTo ( TextWriter writer ) => PrettyPrint(writer, this);

        private static void PrettyPrint ( TextWriter writer, SyntaxNode node, string indent = "", bool last = true )
        {
            bool isToConsole = writer == Console.Out;
            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            writer.Write($"{indent}{( last ? "└──" : "├──" )}");
            if (isToConsole)
                Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
            writer.Write(node.Kind);
            if (node is SyntaxToken t && t.Value != null)
                writer.Write($": {t.Value}");
            if (isToConsole)
                Console.ResetColor();
            indent += last ? "   " : "│  ";
            writer.WriteLine();
            foreach (SyntaxNode child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == node.GetChildren().LastOrDefault());
        }

        public override string ToString ( )
        {
            using StringWriter writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}