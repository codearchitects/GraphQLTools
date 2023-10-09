using Microsoft.VisualStudio.Text;

namespace GraphQLTools.Syntax
{
    internal interface ISyntaxSpanCollection
    {
        void AddPunctuator(Span span);
        void AddKeyword(Span span);
        void AddOperationName(Span span);
        void AddFragmentName(Span span);
        void AddVariableName(Span span);
        void AddDirectiveName(Span span);
        void AddTypeName(Span span);
        void AddFieldName(Span span);
        void AddAliasedFieldName(Span span);
        void AddArgumentName(Span span);
        void AddObjectFieldName(Span span);
        void AddString(Span span);
        void AddNumber(Span span);
        void AddEnum(Span span);
        void AddName(Span span);
        void AddComment(Span span);
        void AddError(Span span);

        void SetDiagnostic(Span span, string message);
    }
}