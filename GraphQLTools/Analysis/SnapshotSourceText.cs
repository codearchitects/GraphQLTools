using GraphQLTools.Syntax;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;

namespace GraphQLTools.Analysis
{
    internal abstract class SnapshotSourceText : SourceText
    {
        protected SnapshotSourceText(ITextSnapshot snapshot, int start, int end)
            : base(start)
        {
            Snapshot = snapshot;
            Start = start;
            End = end;
        }

        protected ITextSnapshot Snapshot { get; }

        protected override int Start { get; }

        protected override int End { get; }

        protected abstract char MoveNextCore(ref int position);

        protected sealed override char MoveNext(ref int position)
        {
            Debug.Assert(position >= Start && position <= End);

            if (position == End)
                return '\0';

            return MoveNextCore(ref position);
        }
    }
}
