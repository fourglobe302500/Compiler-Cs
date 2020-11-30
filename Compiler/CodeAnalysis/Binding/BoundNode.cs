using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Compiler.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
        public IEnumerable<BoundNode> GetChildren( )
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    BoundNode child = (BoundNode)property.GetValue(this);
                    if (child != null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    foreach (BoundNode child in (IEnumerable<BoundNode>)property.GetValue(this))
                        if (child != null)
                            yield return child;
                }
            }
        }
        private IEnumerable<(string name, object value)> GetProperties( )
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == nameof(Kind) || property.Name == nameof(BoundBinaryExpression.Op) || property.Name == nameof(BoundUnaryExpression.Op))
                    continue;
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                    continue;
                var value = property.GetValue(this);
                if (value != null)
                    yield return (property.Name, value);
            }
        }
        public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);
        private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool last = true)
        {
            bool isToConsole = writer == Console.Out;
            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            writer.Write($"{indent}{(last ? "└──" : "├──")}");
            if (isToConsole)
                Console.ForegroundColor = GetColor(node);
            writer.Write(GetText(node));
            var first = true;
            foreach (var p in node.GetProperties())
            {
                if (first)
                    first = false;
                else
                {
                    if (isToConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    writer.Write(",");
                }
                writer.Write(" ");
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                writer.Write(p.name);
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                writer.Write(" = ");
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                writer.Write(p.value);
            }
            if (isToConsole)
                Console.ResetColor();
            indent += last ? "   " : "│  ";
            writer.WriteLine();
            foreach (BoundNode child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == node.GetChildren().LastOrDefault());
        }
        private static string GetText(BoundNode node) => node switch {
            BoundBinaryExpression b => b.Op.Kind.ToString() + "Expression",
            BoundUnaryExpression u => u.Op.Kind.ToString() + "Expression",
            _ => node.Kind.ToString()
        };
        private static ConsoleColor GetColor(BoundNode node) => node switch {
            BoundExpression => ConsoleColor.Blue,
            BoundStatement => ConsoleColor.Cyan,
            _ => ConsoleColor.Yellow
        };
        public override string ToString( )
        {
            using StringWriter writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}