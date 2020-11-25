using Compiler.CodeAnalysis.Text;

namespace Compiler.CodeAnalysis
{
    public sealed class Diagnostic
    {
        public Diagnostic(TextSpan spam, string massage)
        {
            Span = spam;
            Massage = massage;
        }

        public TextSpan Span { get; }
        public string Massage { get; }

        public override string ToString() => Massage;
    }
}