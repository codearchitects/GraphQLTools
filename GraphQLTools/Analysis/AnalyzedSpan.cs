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

        protected AnalyzedSpan(SyntaxKind syntaxKind, bool isUnterminated)
        {
            SyntaxKind = syntaxKind;
            IsUnterminated = isUnterminated;
            ParentSpanStart = -1;
            ExpressionSpanStart = -1;
        }

        public SyntaxKind SyntaxKind { get; }

        public bool IsUnterminated { get; }

        public int Start => ExpressionSpanStart;

        public int Length => ExpressionSpanLength;

        public int End => ExpressionSpanEnd;

        protected int ParentSpanStart { get; private set; }

        protected int ParentSpanLength { get; private set; }

        protected int ParentSpanEnd => ParentSpanStart + ParentSpanLength;

        protected int ExpressionSpanStart { get; private set; }

        protected int ExpressionSpanLength { get; private set; }

        protected int ExpressionSpanEnd => ExpressionSpanStart + ExpressionSpanLength;

        protected abstract int GqlSpanStart { get; }

        protected abstract int GqlSpanEnd { get; }

        protected abstract SourceText CreateSource(ITextSnapshot snapshot);

        public void Reparse(ITextSnapshot snapshot, CSharpSyntaxNode parent, LiteralExpressionSyntax expression, ref SyntaxSpanList spans)
        {
            TextSpan parentSpan = parent.Span;
            TextSpan expressionSpan = expression.Span;

            ParentSpanStart = parentSpan.Start;
            ParentSpanLength = parentSpan.Length;
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
            int parentStart = ParentSpanStart;
            int parentEnd = ParentSpanEnd;
            int gqlStart = GqlSpanStart;
            int gqlEnd = GqlSpanEnd;

            for (int i = changes.Count - 1; i >= 0; i--)
            {
                ITextChange change = changes[i];
                int changeStart = change.OldPosition;
                int changeEnd = change.OldEnd;
                int offset = change.Delta;

                if (parentEnd <= changeStart) // If the change is after us, take no action.
                    continue;

                if (changeEnd <= parentStart) // If the change is completely before us, shift the spans' and spans' start positions.
                {
                    ParentSpanStart += offset;
                    ExpressionSpanStart += offset;
                    _spans.Shift(offset);
                    continue;
                }

                if (gqlStart <= changeStart && changeEnd <= gqlEnd) // If the change is comprised within the GQL span, adjust the spans' lengths and signal a reparse is needed.
                {
                    ParentSpanLength += offset;
                    ExpressionSpanLength += offset;
                    _spans.Synchronize(change);
                    continue;
                }

                // Otherwise, the analyzed span must be invalidated.
                ParentSpanStart = -1;
                ExpressionSpanStart = -1;
                ParentSpanLength = 0;
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
