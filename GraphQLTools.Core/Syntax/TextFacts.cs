using System.Runtime.CompilerServices;

namespace GraphQLTools.Syntax;

internal static class TextFacts
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlpha(char c) => c is
        >= 'a' and <= 'z' or
        >= 'A' and <= 'Z' or
        '_';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit(char c) => c is >= '0' and <= '9';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIgnored(char c) => c is ' ' or '\t' or ',' || IsNewlineCharacter(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNewlineCharacter(char c) => c is '\n' or '\r';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExponentIndicator(char c) => c is 'e' or 'E';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSign(char c) => c is '+' or '-';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWordLike(char c) => IsAlphaNumeric(c) || IsSign(c) || c is '.';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEscaped(char c) => c is '"' or '\\' or '/' or 'b' or 'f' or 'n' or 'r' or 't';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexDigit(char c)
    {
        return c is
            >= '0' and <= '9' or
            >= 'A' and <= 'F' or
            >= 'a' and <= 'f';
    }
}
