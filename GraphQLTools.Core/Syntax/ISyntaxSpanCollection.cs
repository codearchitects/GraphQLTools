using Microsoft.CodeAnalysis.Text;

namespace GraphQLTools.Syntax;

internal interface ISyntaxSpanCollection
{
    void AddPunctuator(TextSpan span);
    void AddKeyword(TextSpan span);
    void AddOperationName(TextSpan span);
    void AddFragmentName(TextSpan span);
    void AddVariableName(TextSpan span);
    void AddDirectiveName(TextSpan span);
    void AddTypeName(TextSpan span);
    void AddFieldName(TextSpan span);
    void AddAliasedFieldName(TextSpan span);
    void AddArgumentName(TextSpan span);
    void AddObjectFieldName(TextSpan span);
    void AddString(TextSpan span);
    void AddNumber(TextSpan span);
    void AddEnum(TextSpan span);
    void AddName(TextSpan span);
    void AddComment(TextSpan span);
    void AddError(TextSpan span);

    void SetDiagnostic(TextSpan span, string message);
}
