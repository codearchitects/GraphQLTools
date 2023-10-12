namespace GraphQLTools.Syntax;

internal enum SyntaxKind // Not really representing syntax constructs, but whatever.
{
    Punctuator,
    Keyword,
    OperationName,
    FragmentName,
    VariableName,
    DirectiveName,
    TypeName,
    FieldName,
    AliasedFieldName,
    ArgumentName,
    ObjectFieldName,
    String,
    Number,
    Enum,
    Name,
    Comment,
    Error
}