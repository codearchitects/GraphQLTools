using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GraphQLTools.Tagging
{
    internal class TaggerSemaphore
    {
        private readonly ConcurrentDictionary<ITextSnapshot, CancellationTokenSource> _sources;
        private readonly ConcurrentQueue<string> _versions;
        private readonly Func<bool> _spinningCondition;
        private bool _hasPendingChanges;

        public TaggerSemaphore(ConcurrentDictionary<ITextSnapshot, CancellationTokenSource> cts)
        {
            _sources = cts;
            _versions = new ConcurrentQueue<string>();
            _spinningCondition = () => !_hasPendingChanges;
            _hasPendingChanges = false;
        }

        public void NotifyChangesPending()
        {
            _hasPendingChanges = true;
        }

        public void NotifyChangesHandled()
        {
            _hasPendingChanges = false;
        }

        public void WaitUntilChangesHandled(TimeSpan timeout)
        {
            if (!_hasPendingChanges)
                return;

            if (!SpinWait.SpinUntil(_spinningCondition, timeout))
                throw new OperationCanceledException();
        }

        public void Clear()
        {
            foreach (KeyValuePair<ITextSnapshot, CancellationTokenSource> entry in _sources)
            {
                CancellationTokenSource cts = entry.Value;
                cts.Cancel();
                cts.Dispose();
            }

            _sources.Clear();
        }

        public CancellationToken GetToken(ITextSnapshot snapshot)
        {
            CancellationTokenSource cts = _sources.GetOrAdd(snapshot, _ => new CancellationTokenSource());

            _versions.Enqueue($"[{DateTime.Now:mm:ss:ff}]: Version {snapshot.Version} being processed.");

            return cts.Token;
        }

        public void Cancel(ITextSnapshot snapshot)
        {
            if (!_sources.TryRemove(snapshot, out CancellationTokenSource cts))
                return;

            cts.Cancel();
            cts.Dispose();

            _versions.Enqueue($"[{DateTime.Now:mm:ss:ff}]: Version {snapshot.Version} cancelled {(ThreadHelper.CheckAccess() ? "on UI thread" : "on background thread")}");
        }

        public static TaggerSemaphore Create()
        {
            return new TaggerSemaphore(new ConcurrentDictionary<ITextSnapshot, CancellationTokenSource>());
        }
    }
}
