using GraphQLTools.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;
using static GraphQLTools.Syntax.TextFacts;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace GraphQLTools.Analysis
{
    internal sealed class SimpleStringLiteralAnalyzedSpan : AnalyzedSpan
    {
        public SimpleStringLiteralAnalyzedSpan(SyntaxKind syntaxKind, bool isUnterminated)
            : base(syntaxKind, isUnterminated)
        {
        }

        protected override int GqlSpanStart => ExpressionSpanStart + 1;

        protected override int GqlSpanEnd => IsUnterminated
            ? ExpressionSpanEnd
            : ExpressionSpanEnd - 1;

        protected override SourceText CreateSource(ITextSnapshot snapshot)
        {
            return new SimpleStringLiteralSourceText(snapshot, GqlSpanStart, GqlSpanEnd);
        }

        public static SimpleStringLiteralAnalyzedSpan Create(ITextSnapshot snapshot, LiteralExpressionSyntax expression)
        {
            bool isUnterminated = !IsClosingSequence(snapshot, expression.Span.End - 1);
            return new SimpleStringLiteralAnalyzedSpan(expression.Token.Kind(), isUnterminated);
        }

        private static bool IsClosingSequence(ITextSnapshot snapshot, int position)
        {
            if (snapshot[position] != '"')
                return false;

            position--;
            int backslashCount = 0;
            while (snapshot[position] == '\\')
            {
                backslashCount++;
                position--;
            }

            return backslashCount % 2 == 0;
        }

        private sealed class SimpleStringLiteralSourceText : SnapshotSourceText // Based on Roslyn's lexer
        {
            private char _surrogateChar;

            public SimpleStringLiteralSourceText(ITextSnapshot snapshot, int start, int end)
                : base(snapshot, start, end)
            {
            }

            protected override char MoveNextCore(ref int position)
            {
                char current;

                if (_surrogateChar != default)
                {
                    current = _surrogateChar;
                    _surrogateChar = default;
                    position++;
                    return current;
                }

                current = Snapshot[position];
                position++;

                if (current == '"')
                {
                    Debug.Fail("Found unescaped double quotes.");
                    return InvalidChar;
                }

                if (current != '\\')
                    return current;

                if (position == End)
                    return InvalidChar;

                current = Snapshot[position];
                position++;

                switch (current)
                {
                    // Escaped characters that translate to themselves.
                    case '\'':
                    case '"':
                    case '\\':
                        return current;

                    // Translate escapes as per C# spec 2.4.4.4.
                    case '0':
                        return '\u0000';

                    case 'a':
                        return '\u0007';

                    case 'b':
                        return '\u0008';

                    case 'f':
                        return '\u000c';

                    case 'n':
                        return '\u000a';

                    case 'r':
                        return '\u000d';

                    case 't':
                        return '\u0009';

                    case 'v':
                        return '\u000b';

                    case 'x':
                        return GetUtf16EscapedChar(ref position, variableLength: true);

                    case 'u':
                        return GetUtf16EscapedChar(ref position, variableLength: false);

                    case 'U':
                        return GetUtf32EscapedChar(ref position, out _surrogateChar);

                    default:
                        return InvalidChar;
                }
            }

            private char GetUtf16EscapedChar(ref int position, bool variableLength)
            {
                if (position == End)
                    return InvalidChar;

                char current = Snapshot[position];
                position++;

                if (!IsHexDigit(current))
                    return InvalidChar;

                int codepoint = HexValue(current);

                for (int i = 0; i < 3; i++)
                {
                    if (position == End)
                        return InvalidChar;

                    current = Snapshot[position];
                    position++;

                    if (!IsHexDigit(current))
                    {
                        if (!variableLength)
                            return InvalidChar;

                        break;
                    }

                    codepoint = (codepoint << 4) + HexValue(current);
                }

                return (char)codepoint;
            }

            private char GetUtf32EscapedChar(ref int position, out char surrogateChar)
            {
                surrogateChar = default;

                if (position == End)
                    return InvalidChar;

                char current = Snapshot[position];
                position++;

                if (!IsHexDigit(current))
                    return InvalidChar;

                int codepoint = HexValue(current);

                for (int i = 0; i < 7; i++)
                {
                    if (position == End)
                        return InvalidChar;

                    current = Snapshot[position];
                    position++;

                    if (!IsHexDigit(current))
                        return InvalidChar;

                    codepoint = (codepoint << 4) + HexValue(current);
                }

                if (codepoint > 0x0010FFFF)
                    return InvalidChar;

                if (codepoint >= 0x00010000)
                {
                    surrogateChar = (char)((codepoint - 0x00010000) % 0x0400 + 0xDC00);
                    return (char)((codepoint - 0x00010000) / 0x0400 + 0xD800);
                }

                return (char)codepoint;
            }
        }

        private static int HexValue(char c)
        {
            Debug.Assert(IsHexDigit(c));

            return (c >= '0' && c <= '9') ? c - '0' : (c & 0xDF) - 'A' + 10;
        }
    }
}
