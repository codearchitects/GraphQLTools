using GraphQLTools.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;

namespace GraphQLTools.Analysis
{
    internal sealed class VerbatimStringLiteralAnalyzedSpan : AnalyzedSpan
    {
        public VerbatimStringLiteralAnalyzedSpan(Microsoft.CodeAnalysis.CSharp.SyntaxKind syntaxKind, bool isUnterminated)
            : base(syntaxKind, isUnterminated)
        {
        }

        protected override int GqlSpanStart => ExpressionSpanStart + 2;

        protected override int GqlSpanEnd => IsUnterminated
            ? ExpressionSpanEnd
            : ExpressionSpanEnd - 1;

        protected override SourceText CreateSource(ITextSnapshot snapshot)
        {
            return new VerbatimStringLiteralSourceText(snapshot, GqlSpanStart, GqlSpanEnd);
        }

        public static VerbatimStringLiteralAnalyzedSpan Create(ITextSnapshot snapshot, LiteralExpressionSyntax expression)
        {
            bool isUnterminated = !IsClosingSequence(snapshot, expression.Span.End - 1);
            return new VerbatimStringLiteralAnalyzedSpan(expression.Token.Kind(), isUnterminated);
        }

        private static bool IsClosingSequence(ITextSnapshot snapshot, int position)
        {
            if (snapshot[position] != '"')
                return false;

            position--;
            int doubleQuotesCount = 0;
            while (snapshot[position] == '"')
            {
                doubleQuotesCount++;
                position--;
            }

            return doubleQuotesCount % 2 == 0;
        }

        private sealed class VerbatimStringLiteralSourceText : SnapshotSourceText
        {
            public VerbatimStringLiteralSourceText(ITextSnapshot snapshot, int start, int end)
                : base(snapshot, start, end)
            {
            }

            protected override char MoveNextCore(ref int position)
            {
                char current = Snapshot[position];
                position++;

                if (current == '"')
                {
                    Debug.Assert(position != End && Snapshot[position] == '"', "Found unescaped double quotes.");

                    position++;
                }

                return current;
            }
        }
    }
}
