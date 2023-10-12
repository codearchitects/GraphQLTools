using GraphQLTools.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace GraphQLTools.Analysis
{
    internal class SyntaxSpanList : ISyntaxSpanCollection
    {
        public static readonly SyntaxSpanListSlice EmptySlice = new SyntaxSpanListSlice(new SyntaxSpanList(), default);

        private readonly List<SyntaxSpan> _list;

        private SyntaxSpanList()
        {
            _list = new List<SyntaxSpan>();
        }

        public SyntaxSpanList(int capacity)
        {
            _list = new List<SyntaxSpan>(capacity);
        }

        public int Count => _list.Count;

        public DiagnosticSpan? DiagnosticSpan { get; private set; }

        public SyntaxSpan this[int index] => _list[index];

        public void Shift(int offset)
        {
            if (DiagnosticSpan is DiagnosticSpan diagnosticSpan)
            {
                DiagnosticSpan = diagnosticSpan.Shift(offset);
            }

            for (int i = 0; i < _list.Count; i++)
            {
                _list[i] = _list[i].Shift(offset);
            }
        }

        public void Synchronize(ITextChange change)
        {
            int changeStart = change.OldPosition;
            int changeEnd = change.OldEnd;
            int offset = change.Delta;

            for (int i = 0; i < _list.Count; i++)
            {
                SyntaxSpan token = _list[i];
                int tokenStart = token.Start;
                int tokenEnd = token.End;

                if (tokenEnd < changeStart) // If the change is completely after the token, take no action.
                    continue;

                if (changeEnd <= tokenStart) // If the change is before the token, shift the and token's start positions.
                {
                    _list[i] = token.Shift(offset);
                    continue;
                }

                // Otherwise, we adjust the token's span.
                int newLength;

                if (changeStart <= tokenStart)
                {
                    newLength = changeEnd < tokenEnd
                      ? tokenEnd - changeStart + offset
                      : 0;

                    _list[i] = token.WithSpan(changeStart, newLength);
                    continue;
                }

                newLength = changeEnd < tokenEnd
                    ? token.Length + offset
                    : change.NewLength + changeStart - tokenStart;

                _list[i] = token.WithSpan(tokenStart, newLength);
            }

            if (DiagnosticSpan is DiagnosticSpan diagnosticSpan)
            {
                if (diagnosticSpan.End < changeStart || diagnosticSpan.Start < changeEnd)
                    return;

                DiagnosticSpan = diagnosticSpan.Shift(offset);
            }
        }

        public void Clear()
        {
            _list.Clear();
            DiagnosticSpan = null;
        }

        void ISyntaxSpanCollection.AddPunctuator(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Punctuator, span));
        }

        void ISyntaxSpanCollection.AddOperationName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.OperationName, span));
        }

        void ISyntaxSpanCollection.AddFragmentName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.FragmentName, span));
        }

        void ISyntaxSpanCollection.AddKeyword(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Keyword, span));
        }

        void ISyntaxSpanCollection.AddVariableName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.VariableName, span));
        }

        void ISyntaxSpanCollection.AddDirectiveName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.DirectiveName, span));
        }

        void ISyntaxSpanCollection.AddTypeName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.TypeName, span));
        }

        void ISyntaxSpanCollection.AddFieldName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.FieldName, span));
        }

        void ISyntaxSpanCollection.AddAliasedFieldName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.AliasedFieldName, span));
        }

        void ISyntaxSpanCollection.AddArgumentName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.ArgumentName, span));
        }

        void ISyntaxSpanCollection.AddObjectFieldName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.ObjectFieldName, span));
        }

        void ISyntaxSpanCollection.AddString(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.String, span));
        }

        void ISyntaxSpanCollection.AddNumber(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Number, span));
        }

        void ISyntaxSpanCollection.AddEnum(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Enum, span));
        }

        void ISyntaxSpanCollection.AddName(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Name, span));
        }

        void ISyntaxSpanCollection.AddComment(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Comment, span));
        }

        void ISyntaxSpanCollection.AddError(TextSpan span)
        {
            VerifyAtEnd(span);

            _list.Add(SyntaxSpan.Create(SyntaxKind.Error, span));
        }

        void ISyntaxSpanCollection.SetDiagnostic(TextSpan span, string message)
        {
            Debug.Assert(DiagnosticSpan == null);

            DiagnosticSpan = Syntax.DiagnosticSpan.Create(message, span);
        }

        [Conditional("DEBUG")]
        private void VerifyAtEnd(TextSpan span)
        {
            Debug.Assert(_list.Count == 0 || span.Start >= _list[_list.Count - 1].End, "Attempted to add a tagged span in the wrong order.");
        }
    }
}