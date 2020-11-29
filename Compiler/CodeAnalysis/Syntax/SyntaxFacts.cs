using System;
using System.Collections.Generic;
using System.Linq;
namespace Compiler.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) => kind switch {
            SyntaxKind.PlusToken => 7,
            SyntaxKind.MinusToken => 7,
            SyntaxKind.ExclamationToken => 7,
            _ => 0,
        };
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) => kind switch {
            SyntaxKind.PercentToken => 6,
            SyntaxKind.HatToken => 6,
            SyntaxKind.SlashToken => 5,
            SyntaxKind.StarToken => 5,
            SyntaxKind.MinusToken => 4,
            SyntaxKind.PlusToken => 4,
            SyntaxKind.DoubleEqualsToken => 3,
            SyntaxKind.NotEqualsToken => 3,
            SyntaxKind.LessOrEqualsThenToken => 3,
            SyntaxKind.LessThenToken => 3,
            SyntaxKind.GreaterOrEqualsThenToken => 3,
            SyntaxKind.GreaterThenToken => 3,
            SyntaxKind.LogicalAndToken => 2,
            SyntaxKind.LogicalOrToken => 1,
            _ => 0,
        };
        public static SyntaxKind GetKeywordKind(string text) => text switch {
            "true" => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "var" => SyntaxKind.VarKeyword,
            "def" => SyntaxKind.DefKeyword,
            "if" => SyntaxKind.IfKeyword,
            "else" => SyntaxKind.ElseKeyword,
            "for" => SyntaxKind.ForKeyword,
            "while" => SyntaxKind.WhileKeyword,
            _ => SyntaxKind.IdentifierToken,
        };
        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds( )
            => ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind))).Where(kind => GetUnaryOperatorPrecedence(kind) > 0);
        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds( )
            => ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind))).Where(kind => GetBinaryOperatorPrecedence(kind) > 0);
        public static string GetText(SyntaxKind kind) => kind switch {
            SyntaxKind.CloseParenthesisToken => ")",
            SyntaxKind.OpenParenthesisToken => "(",
            SyntaxKind.OpenBraceToken => "{",
            SyntaxKind.CloseBraceToken => "}",
            SyntaxKind.PlusToken => "+",
            SyntaxKind.MinusToken => "-",
            SyntaxKind.StarToken => "*",
            SyntaxKind.SlashToken => "/",
            SyntaxKind.PercentToken => "%",
            SyntaxKind.HatToken => "^",
            SyntaxKind.TrueKeyword => "true",
            SyntaxKind.FalseKeyword => "false",
            SyntaxKind.VarKeyword => "var",
            SyntaxKind.DefKeyword => "def",
            SyntaxKind.IfKeyword => "if",
            SyntaxKind.ElseKeyword => "else",
            SyntaxKind.ForKeyword => "for",
            SyntaxKind.WhileKeyword => "while",
            SyntaxKind.AssigmentToken => "=",
            SyntaxKind.DoubleEqualsToken => "==",
            SyntaxKind.NotEqualsToken => "!=",
            SyntaxKind.ExclamationToken => "!",
            SyntaxKind.LogicalOrToken => "||",
            SyntaxKind.LogicalAndToken => "&&",
            SyntaxKind.LessOrEqualsThenToken => "<=",
            SyntaxKind.LessThenToken => "<",
            SyntaxKind.GreaterOrEqualsThenToken => ">=",
            SyntaxKind.GreaterThenToken => ">",
            _ => null,
        };
    }
}