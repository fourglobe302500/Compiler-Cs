using System.Collections.Generic;
using System.Linq;

namespace Compiler.CodeAnalysis.Syntax
{
  public sealed class SyntaxToken : SyntaxNode
  {
    public SyntaxToken(
      SyntaxKind kind,
      int _position,
      string text,
      object value = null)
    {
      Kind = kind;
      Position = _position;
      Text = text;
      Value = value;
    }

    public static SyntaxKind AssigmentToken { get; internal set; }
    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public string Text { get; }
    public object Value { get; }
    public TextSpan Span => new TextSpan(Position, Text.Length);

    public override IEnumerable<SyntaxNode> GetChildren() =>
    Enumerable.Empty<SyntaxNode>();
  }

}