namespace GraphQLTools.Syntax;

internal static class ErrorMessages
{
    private const string s_prefix = "GraphQL syntax error - ";

    public const string UnterminatedString         = s_prefix + "Unterminated string.";
    public const string InvalidString              = s_prefix + "Invalid string.";
    public const string InvalidNumber              = s_prefix + "Invalid number.";
    public const string ExpectedName               = s_prefix + "Expected name.";
    public const string ExpectedDefinition         = s_prefix + "Expected definition.";
    public const string ExpectedSelectionSet       = s_prefix + "Expected selection set.";
    public const string ExpectedSelection          = s_prefix + "Expected selection.";
    public const string ExpectedRightParenthesis   = s_prefix + "Expected ')'.";
    public const string ExpectedRightBracket       = s_prefix + "Expected ']'.";
    public const string ExpectedRightBrace         = s_prefix + "Expected '}'";
    public const string ExpectedType               = s_prefix + "Expected type.";
    public const string ExpectedTypeName           = s_prefix + "Expected type name.";
    public const string ExpectedFragment           = s_prefix + "Expected fragment.";
    public const string ExpectedArgument           = s_prefix + "Expected argument.";
    public const string ExpectedColon              = s_prefix + "Expected ':'.";
    public const string ExpectedValue              = s_prefix + "Expected value.";
    public const string ExpectedVariableDefinition = s_prefix + "Expected variable definition.";
    public const string ExpectedFragmentName       = s_prefix + "Expected fragment name.";
    public const string ExpectedTypeCondition      = s_prefix + "Expected type condition.";
    public const string AnonymousOperationError    = s_prefix + "An anonymous operation must be the only defined operation.";

    public static string UnexpectedCharacter(char c)
    {
        return char.IsControl(c)
            ? s_prefix + "Unexpected character."
            : s_prefix + $"Unexpected character: '{c}'.";
    }
}
