using System;
using System.Collections.Generic;

namespace Compiler.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) 
        {
            switch(kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.ExclamationToken:
                    return 7;
                default: 
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) 
        {
            switch(kind)
            {
                case SyntaxKind.PercentToken:
                case SyntaxKind.HatToken: 
                    return 6;
                case SyntaxKind.SlashToken:
                case SyntaxKind.StarToken: 
                    return 5;
                case SyntaxKind.MinusToken:
                case SyntaxKind.PlusToken:
                    return 4;
                case SyntaxKind.DoubleEqualsToken:
                case SyntaxKind.NotEqualsToken:
                case SyntaxKind.LessOrEqualsThenToken:
                case SyntaxKind.LessThenToken:
                case SyntaxKind.GreaterOrEqualsThenToken:
                case SyntaxKind.GreaterThenToken:
                    return 3;
                case SyntaxKind.LogicalAndToken: 
                    return 2;
                case SyntaxKind.LogicalOrToken:
                    return 1;
                default: 
                    return 0;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch(text)
            {
                case "true": 
                    return SyntaxKind.TrueKeyword;
                case "false": 
                    return SyntaxKind.FalseKeyword;
                default: 
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            foreach (var kind in (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind)))
                if (GetUnaryOperatorPrecedence(kind) > 0)
                    yield return kind;
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            foreach (var kind in (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind)))
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
        }

        public static string GetText(SyntaxKind kind)
        {
            switch(kind)
            {
                case SyntaxKind.CloseParenthesisToken: 
                    return ")";
                case SyntaxKind.OpenParenthesisToken: 
                    return "(";
                case SyntaxKind.PlusToken: 
                    return "+";
                case SyntaxKind.MinusToken: 
                    return "-";
                case SyntaxKind.StarToken: 
                    return "*";
                case SyntaxKind.SlashToken: 
                    return "/";
                case SyntaxKind.PercentToken: 
                    return "%";
                case SyntaxKind.HatToken: 
                    return "^";
                case SyntaxKind.TrueKeyword: 
                    return "true";
                case SyntaxKind.FalseKeyword: 
                    return "false";
                case SyntaxKind.AssigmentToken: 
                    return "=";
                case SyntaxKind.DoubleEqualsToken: 
                    return "==";
                case SyntaxKind.NotEqualsToken: 
                    return "!=";
                case SyntaxKind.ExclamationToken: 
                    return "!";
                case SyntaxKind.LogicalOrToken: 
                    return "||";
                case SyntaxKind.LogicalAndToken: 
                    return "&&";
                case SyntaxKind.LessOrEqualsThenToken: 
                    return "<=";
                case SyntaxKind.LessThenToken: 
                    return "<";
                case SyntaxKind.GreaterOrEqualsThenToken: 
                    return ">=";
                case SyntaxKind.GreaterThenToken: 
                    return ">";
                default: 
                    return null;
            }
        }
    }
}