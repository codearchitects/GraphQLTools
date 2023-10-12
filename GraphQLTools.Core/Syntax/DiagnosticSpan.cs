using Microsoft.CodeAnalysis.Text;

namespace GraphQLTools.Syntax;

internal readonly struct DiagnosticSpan
{
    private DiagnosticSpan(string message, int start, int length)
    {
        Message = message;
        Start = start;
        Length = length;
    }

    public string Message { get; }

    public int Start { get; }

    public int Length { get; }

    public int End => Start + Length;

    public DiagnosticSpan Shift(int offset) => new DiagnosticSpan(Message, Start + offset, Length);

    public static DiagnosticSpan Create(string message, TextSpan span) => new DiagnosticSpan(message, span.Start, span.Length);
}
