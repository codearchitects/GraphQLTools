using GraphQLTools.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;
using SourceText = GraphQLTools.Syntax.SourceText;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace GraphQLTools.Analysis
{
    internal abstract class AnalyzedSpan
    {
        private volatile SyntaxSpanList _spans;

        protected AnalyzedSpan(SyntaxKind syntaxKind)
        {
            SyntaxKind = syntaxKind;
            InvocationSpanStart = -1;
            ExpressionSpanStart = -1;
        }

        public SyntaxKind SyntaxKind { get; }

        public int Start => ExpressionSpanStart;

        public int Length => ExpressionSpanLength;

        public int End => ExpressionSpanEnd;

        protected int InvocationSpanStart { get; private set; }

        protected int InvocationSpanLength { get; private set; }

        protected int InvocationSpanEnd => InvocationSpanStart + InvocationSpanLength;

        protected int ExpressionSpanStart { get; private set; }

        protected int ExpressionSpanLength { get; private set; }

        protected int ExpressionSpanEnd => ExpressionSpanStart + ExpressionSpanLength;

        protected abstract int GqlSpanStart { get; }

        protected abstract int GqlSpanEnd { get; }

        protected abstract SourceText CreateSource(ITextSnapshot snapshot);

        public void Reparse(ITextSnapshot snapshot, InvocationExpressionSyntax invocation, LiteralExpressionSyntax expression, ref SyntaxSpanList spans)
        {
            TextSpan invocationSpan = invocation.Span;
            TextSpan expressionSpan = expression.Span;

            InvocationSpanStart = invocationSpan.Start;
            InvocationSpanLength = invocationSpan.Length;
            ExpressionSpanStart = expressionSpan.Start;
            ExpressionSpanLength = expressionSpan.Length;

            SourceText source = CreateSource(snapshot);

            try
            {
                lock (spans)
                {
                    GqlParser.Parse(source, spans);
                }
            }
            finally
            {
                (_spans, spans) = (spans, _spans);
            }
        }

        public SyntaxSpanList ReturnSpanList()
        {
            Debug.Assert(_spans != null);

            SyntaxSpanList result = _spans;
            _spans = null;
            return result;
        }

        public void Synchronize(INormalizedTextChangeCollection changes)
        {
            int invocationStart = InvocationSpanStart;
            int invocationEnd = InvocationSpanEnd;
            int gqlStart = GqlSpanStart;
            int gqlEnd = GqlSpanEnd;

            for (int i = changes.Count - 1; i >= 0; i--)
            {
                ITextChange change = changes[i];
                int changeStart = change.OldPosition;
                int changeEnd = change.OldEnd;
                int offset = change.Delta;

                if (invocationEnd <= changeStart) // If the change is after us, take no action.
                    continue;

                if (changeEnd <= invocationStart) // If the change is completely before us, shift the spans' and spans' start positions.
                {
                    InvocationSpanStart += offset;
                    ExpressionSpanStart += offset;
                    _spans.Shift(offset);
                    continue;
                }

                if (gqlStart <= changeStart && changeEnd <= gqlEnd) // If the change is comprised within the GQL span, adjust the spans' lengths and signal a reparse is needed.
                {
                    InvocationSpanLength += offset;
                    ExpressionSpanLength += offset;
                    _spans.Synchronize(change);
                    continue;
                }

                // Otherwise, the analyzed span must be invalidated.
                InvocationSpanStart = -1;
                ExpressionSpanStart = -1;
                InvocationSpanLength = 0;
                ExpressionSpanLength = 0;
                return;
            }
        }

        public SyntaxSpanListSlice GetSyntaxSpansIn(Span span)
        {
            SyntaxSpanList spans = _spans;
            if (spans == null)
                return SyntaxSpanList.EmptySlice;

            int gqlStart = GqlSpanStart;
            int gqlEnd = GqlSpanEnd;
            if (span.End <= gqlStart || gqlEnd <= span.Start)
                return SyntaxSpanList.EmptySlice;

            return new SyntaxSpanListSlice(spans, span);
        }

        public bool TryGetDiagnosticSpanIn(Span span, out DiagnosticSpan diagnosticSpan)
        {
            DiagnosticSpan? maybeDiagnosticSpan = _spans?.DiagnosticSpan;
            if (maybeDiagnosticSpan == null)
            {
                diagnosticSpan = default;
                return false;
            }

            diagnosticSpan = maybeDiagnosticSpan.Value;

            if (span.IsEmpty)
            {
                int position = span.Start;
                return diagnosticSpan.Start <= position && position <= diagnosticSpan.End;
            }
            else
            {
                return diagnosticSpan.Start < span.End && span.Start < diagnosticSpan.End;
            }
        }

        public static bool TryCreate(ITextSnapshot snapshot, LiteralExpressionSyntax expression, out AnalyzedSpan result)
        {
            result = null;

            switch (expression.Token.Kind())
            {
                case SyntaxKind.StringLiteralToken:
                    switch (snapshot[expression.SpanStart])
                    {
                        case '"':
                            result = SimpleStringLiteralAnalyzedSpan.Create(snapshot, expression);
                            return true;

                        case '@':
                            result = VerbatimStringLiteralAnalyzedSpan.Create(snapshot, expression);
                            return true;

                        default:
                            return false;
                    }

                case SyntaxKind.SingleLineRawStringLiteralToken:
                case SyntaxKind.MultiLineRawStringLiteralToken:
                    result = RawStringLiteralAnalyzedSpan.Create(snapshot, expression);
                    return true;

                default:
                    return false;
            }
        }
    }
}
