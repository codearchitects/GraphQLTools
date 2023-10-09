using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace GraphQLTools.Utils
{
    internal static class VisualStudioExtensions
    {
        public static TextSpan ToRoslynSpan(this Span span)
        {
            return new TextSpan(span.Start, span.Length);
        }
    }
}
