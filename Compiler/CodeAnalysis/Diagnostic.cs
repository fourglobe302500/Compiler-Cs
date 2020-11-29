using Compiler.CodeAnalysis.Text;
namespace Compiler.CodeAnalysis
{
    public sealed class Diagnostic
    {
        public Diagnostic(TextSpan spam, string massage)
        {
            Span = spam;
            Message = massage;
        }
        public TextSpan Span { get; }
        public string Message { get; }
        public override string ToString( ) => Message;
    }
}