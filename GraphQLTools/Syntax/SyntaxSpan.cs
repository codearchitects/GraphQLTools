using Microsoft.VisualStudio.Text;

namespace GraphQLTools.Syntax
{
    internal readonly struct SyntaxSpan
    {
        private SyntaxSpan(SyntaxKind kind, int start, int length)
        {
            Kind = kind;
            Start = start;
            Length = length;
        }

        public SyntaxKind Kind { get; }

        public int Start { get; }

        public int Length { get; }

        public int End => Start + Length;

        public SyntaxSpan Shift(int offset) => new SyntaxSpan(Kind, Start + offset, Length);

        public SyntaxSpan WithSpan(int start, int length) => new SyntaxSpan(Kind, start, length);

        public static SyntaxSpan Create(SyntaxKind kind, Span span) => new SyntaxSpan(kind, span.Start, span.Length);
    }
}
