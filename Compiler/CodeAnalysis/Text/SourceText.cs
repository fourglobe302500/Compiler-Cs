using System.Collections.Immutable;

namespace Compiler.CodeAnalysis.Text
{
    public sealed class SourceText
    {
        private readonly string _text;

        private SourceText(string text)
        {
            _text = text;
            Lines = ParseLine(this, text);
        }

        public ImmutableArray<TextLine> Lines { get; private set; }

        public char this[int index] => _text[index];

        public int Length => _text.Length;

        public int GetLineIndex(int position)
        {
            int lower = 0;
            int upper = Lines.Length - 1;
            while (lower <= upper)
            {
                int index = lower + (upper - lower) / 2;
                int start = Lines[index].Start;
                if (position == start)
                    return index;
                if (position > start)
                    lower = index + 1;
                else
                    upper = index - 1;
            }

            return lower - 1;
        }

        private static ImmutableArray<TextLine> ParseLine(SourceText sourceText, string text)
        {
            ImmutableArray<TextLine>.Builder result = ImmutableArray.CreateBuilder<TextLine>();
            int lineStart = 0;
            int position = 0;

            while (position < text.Length)
            {
                int lineBreakWidth = GetLineBreakWidth(text, position);
                if (lineBreakWidth == 0)
                    position++;
                else
                {
                    AddLine(result, sourceText, lineStart, position, lineBreakWidth);
                    position += lineBreakWidth;
                    lineStart = position;
                }
            }
            if (position >= lineStart)
                AddLine(result, sourceText, lineStart, position, 0);

            return result.ToImmutable();
        }

        private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int lineStart, int position, int lineBreakWidth)
        {
            int lineLength = position - lineStart;
            TextLine line = new TextLine(sourceText, lineStart, lineLength, lineLength + lineBreakWidth);
            result.Add(line);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            char c = text[position];
            char l = position + 1 >= text.Length ? '\0' : text[position + 1];
            if (c == '\r' && l == '\n')
                return 2;
            if (c == '\r' || c == '\n')
                return 1;
            return 0;
        }

        public static SourceText From(string text) => new SourceText(text);

        public override string ToString() => _text;

        public string ToString(int start, int length) => _text.Substring(start, length);

        public string ToString(TextSpan span) => _text.Substring(span.Start, span.Length);
    }
}