using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace GraphQLTools.Syntax
{
    internal class SyntaxSpanList : List<SyntaxSpan>, ISyntaxSpanCollection
    {
        public static readonly SyntaxSpanListSlice EmptySlice = new SyntaxSpanListSlice(new SyntaxSpanList(), default);

        private SyntaxSpanList()
        {
        }

        public SyntaxSpanList(int capacity)
          : base(capacity)
        {
        }

        public DiagnosticSpan? DiagnosticSpan { get; private set; }

        public void Shift(int offset)
        {
            if (DiagnosticSpan != null)
            {
                DiagnosticSpan = DiagnosticSpan.Value.Shift(offset);
            }

            for (int i = 0; i < Count; i++)
            {
                this[i] = this[i].Shift(offset);
            }
        }

        public new void Clear()
        {
            base.Clear();
            DiagnosticSpan = null;
        }

        void ISyntaxSpanCollection.AddPunctuator(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Punctuator, span));
        }

        void ISyntaxSpanCollection.AddOperationName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.OperationName, span));
        }

        void ISyntaxSpanCollection.AddFragmentName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.FragmentName, span));
        }

        void ISyntaxSpanCollection.AddKeyword(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Keyword, span));
        }

        void ISyntaxSpanCollection.AddVariableName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.VariableName, span));
        }

        void ISyntaxSpanCollection.AddDirectiveName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.DirectiveName, span));
        }

        void ISyntaxSpanCollection.AddTypeName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.TypeName, span));
        }

        void ISyntaxSpanCollection.AddFieldName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.FieldName, span));
        }

        void ISyntaxSpanCollection.AddAliasedFieldName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.AliasedFieldName, span));
        }

        void ISyntaxSpanCollection.AddArgumentName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.ArgumentName, span));
        }

        void ISyntaxSpanCollection.AddObjectFieldName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.ObjectFieldName, span));
        }

        void ISyntaxSpanCollection.AddString(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.String, span));
        }

        void ISyntaxSpanCollection.AddNumber(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Number, span));
        }

        void ISyntaxSpanCollection.AddEnum(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Enum, span));
        }

        void ISyntaxSpanCollection.AddName(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Name, span));
        }

        void ISyntaxSpanCollection.AddComment(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Comment, span));
        }

        void ISyntaxSpanCollection.AddError(Span span)
        {
            VerifyAtEnd(span);

            Add(SyntaxSpan.Create(SyntaxKind.Error, span));
        }

        void ISyntaxSpanCollection.SetDiagnostic(Span span, string message)
        {
            Debug.Assert(DiagnosticSpan == null);

            DiagnosticSpan = Syntax.DiagnosticSpan.Create(message, span);
        }

        [Conditional("DEBUG")]
        private void VerifyAtEnd(Span span)
        {
            Debug.Assert(Count == 0 || span.Start >= this[Count - 1].End, "Attempted to add a tagged span in the wrong order.");
        }
    }
}
