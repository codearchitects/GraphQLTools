using GraphQLTools.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;

namespace GraphQLTools.Analysis
{
    internal sealed class RawStringLiteralAnalyzedSpan : AnalyzedSpan
    {
        private readonly int _quoteCount;

        public RawStringLiteralAnalyzedSpan(Microsoft.CodeAnalysis.CSharp.SyntaxKind syntaxKind, bool isUnterminated, int quoteCount)
            : base(syntaxKind, isUnterminated)
        {
            _quoteCount = quoteCount;
        }

        protected override int GqlSpanStart => ExpressionSpanStart + _quoteCount;

        protected override int GqlSpanEnd => IsUnterminated
            ? ExpressionSpanEnd
            : ExpressionSpanEnd - _quoteCount;

        protected override SourceText CreateSource(ITextSnapshot snapshot)
        {
            return new RawStringLiteralSourceText(snapshot, GqlSpanStart, GqlSpanEnd);
        }

        public static RawStringLiteralAnalyzedSpan Create(ITextSnapshot snapshot, LiteralExpressionSyntax expression)
        {
            int index = expression.SpanStart;
            int openingQuoteCount = 0;
            while (snapshot[index] == '"')
            {
                openingQuoteCount++;
                index++;
            }

            index = expression.Span.End - 1;
            int closingQuoteCount = 0;
            while (snapshot[index] == '"')
            {
                closingQuoteCount++;
                index--;
            }

            return new RawStringLiteralAnalyzedSpan(expression.Token.Kind(), openingQuoteCount != closingQuoteCount, openingQuoteCount);
        }

        private sealed class RawStringLiteralSourceText : SnapshotSourceText
        {
            public RawStringLiteralSourceText(ITextSnapshot snapshot, int start, int end)
                : base(snapshot, start, end)
            {
            }

            protected override char MoveNextCore(ref int position)
            {
                char current = Snapshot[position];
                position++;

                return current;
            }
        }
    }
}
