using System.Runtime.CompilerServices;

namespace GraphQLTools.Syntax
{
    internal static class TextFacts
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlpha(char c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(char c) => c >= '0' && c <= '9';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIgnored(char c) => c == ' ' || c == '\t' || c == ',' || IsNewlineCharacter(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNewlineCharacter(char c) => c == '\n' || c == '\r';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExponentIndicator(char c) => c == 'e' || c == 'E';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSign(char c) => c == '+' || c == '-';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWordLike(char c) => IsAlphaNumeric(c) || IsSign(c) || c is '.';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEscaped(char c) => c == '"' || c == '\\' || c == '/' || c == 'b' || c == 'f' || c == 'n' || c == 'r' || c == 't';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexDigit(char c)
        {
            return
              (c >= '0' && c <= '9') ||
              (c >= 'A' && c <= 'F') ||
              (c >= 'a' && c <= 'f');
        }
    }
}
