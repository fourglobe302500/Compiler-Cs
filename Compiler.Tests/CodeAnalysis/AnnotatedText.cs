using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;

using Compiler.CodeAnalysis.Text;
using System.Text;

namespace Compiler.Tests.CodeAnalysis
{
    internal sealed class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }
        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }
        public static AnnotatedText Parse(string text)
        {
            text = Clean(text);
            var textBuider = new StringBuilder();
            var spanBuider = ImmutableArray.CreateBuilder<TextSpan>();
            var startStack = new Stack<int>();
            int position = 0;
            foreach (var c in text)
            {
                if (c == '[')
                    startStack.Push(position);
                else if (c == ']')
                {
                    if (startStack.Count == 0)
                        throw new ArgumentException("Too many ']' in text", nameof(text));
                    spanBuider.Add(TextSpan.FromBounds(startStack.Pop(), position));
                }
                else
                {
                    position++;
                    textBuider.Append(c);
                }
            }
            if (startStack.Count > 0)
                throw new ArgumentException("Missing ']' in text", nameof(text));
            return new AnnotatedText(textBuider.ToString(), spanBuider.ToImmutable());
        }
        private static string Clean(string text)
        {
            var lines = UnindentLines(text);
            return string.Join(Environment.NewLine, lines);
        }
        public static string[] UnindentLines(string text)
        {
            var lines = new List<string>();
            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }
            var minIndentantion = int.MaxValue;
            lines.ForEach(line => {
                if (line.Trim().Length != 0)
                    minIndentantion = Math.Min(line.Length - line.TrimStart().Length, minIndentantion);
            });
            lines = lines.Where(line => line.Trim().Length != 0).Select(line => line[minIndentantion..]).ToList();
            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);
            while (lines.Count > 0 && lines[^1].Length == 0)
                lines.RemoveAt(lines.Count - 1);
            return lines.ToArray();
        }
    }
}