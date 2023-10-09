using GraphQLTools.Syntax;
using Microsoft.Extensions.ObjectPool;

namespace GraphQLTools.Analysis
{
    internal class SyntaxSpanListPooledObjectPolicy : IPooledObjectPolicy<SyntaxSpanList>
    {
        public static readonly SyntaxSpanListPooledObjectPolicy Instance = new SyntaxSpanListPooledObjectPolicy();

        private SyntaxSpanListPooledObjectPolicy() { }

        public SyntaxSpanList Create() => new SyntaxSpanList(8);

        public bool Return(SyntaxSpanList obj)
        {
            lock (obj)
            {
                obj.Clear();
            }

            if (obj.Count >= 128)
                return false;

            return true;
        }
    }
}