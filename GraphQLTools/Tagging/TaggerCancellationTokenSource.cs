using Microsoft.VisualStudio.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace GraphQLTools.Tagging
{
    internal readonly struct TaggerCancellationTokenSource
    {
        private readonly ConcurrentDictionary<ITextSnapshot, CancellationTokenSource> _sources;

        public TaggerCancellationTokenSource(ConcurrentDictionary<ITextSnapshot, CancellationTokenSource> cts)
        {
            _sources = cts;
        }

        public void Clear()
        {
            foreach (CancellationTokenSource cts in _sources.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }

            _sources.Clear();
        }

        public CancellationToken GetToken(ITextSnapshot snapshot)
        {
            CancellationTokenSource cts = _sources.GetOrAdd(snapshot, _ => new CancellationTokenSource());

            return cts.Token;
        }

        public void Cancel(ITextSnapshot snapshot)
        {
            if (!_sources.TryRemove(snapshot, out CancellationTokenSource cts))
                return;

            cts.Cancel();
            cts.Dispose();
        }

        public static TaggerCancellationTokenSource Create()
        {
            return new TaggerCancellationTokenSource(new ConcurrentDictionary<ITextSnapshot, CancellationTokenSource>());
        }
    }
}
