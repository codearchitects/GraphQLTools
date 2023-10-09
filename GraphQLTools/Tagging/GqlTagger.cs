using GraphQLTools.Analysis;
using GraphQLTools.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQLTools.Tagging
{
    internal class GqlTagger : ITagger<IClassificationTag>, ITagger<IErrorTag>
    {
        private readonly ITextBuffer2 _buffer;
        private readonly AnalyzedSpanBag _analyzedSpans;
        private readonly GqlTagSpanFactory _tagSpanFactory;
        private readonly TaggerSemaphore _semaphore;

        public GqlTagger(ITextBuffer2 buffer, ObjectPool<SyntaxSpanList> spanListPool, GqlTagSpanFactory tagSpanFactory)
        {
            _buffer = buffer;
            _analyzedSpans = AnalyzedSpanBag.Create(spanListPool);
            _tagSpanFactory = tagSpanFactory;
            _semaphore = TaggerSemaphore.Create();

            buffer.Changed += Buffer_Changed;
            buffer.ChangedOnBackground += Buffer_ChangedOnBackground;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Close()
        {
            _semaphore.Clear();

            _analyzedSpans.Clear();
            
            _buffer.Changed -= Buffer_Changed;
            _buffer.ChangedOnBackground -= Buffer_ChangedOnBackground;
        }

        public void ScanDocument(ProjectId projectId = null)
        {
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (projectId != null && document.Project.Id != projectId)
                return;

            try
            {
                ThreadHelper.JoinableTaskFactory.Run(() => ScanDocumentAsync(snapshot));
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _semaphore.Cancel(snapshot);
            }
        }

        private void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (e.Changes.Count == 0)
                return;

            _semaphore.NotifyChangesPending();
            _semaphore.Cancel(e.Before);

            _analyzedSpans.HandleChanges(e.Changes);
            _semaphore.NotifyChangesHandled();
        }

        private void Buffer_ChangedOnBackground(object sender, TextContentChangedEventArgs e)
        {
            if (e.Changes.Count == 0)
                return;

            ITextSnapshot snapshot = e.After;
            INormalizedTextChangeCollection changes = e.Changes;
            CancellationToken cancellationToken = _semaphore.GetToken(snapshot);

            try
            {
                ThreadHelper.JoinableTaskFactory.Run(() => ScanChangesAsync(snapshot, changes, cancellationToken));
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _semaphore.Cancel(snapshot);
            }
        }

        private async Task ScanChangesAsync(ITextSnapshot snapshot, INormalizedTextChangeCollection changes, CancellationToken cancellationToken)
        {
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document is null || !document.SupportsSyntaxTree || !document.SupportsSemanticModel)
                return;

            await Task.Delay(20, cancellationToken); // Debounce

            SyntaxNode rootNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            _semaphore.WaitUntilChangesHandled(TimeSpan.FromSeconds(100));
            if (cancellationToken.IsCancellationRequested)
                return;

            _analyzedSpans.Rescan(snapshot, changes, rootNode, semanticModel, CancellationToken.None);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
        }

        private async Task ScanDocumentAsync(ITextSnapshot snapshot)
        {
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document is null || !document.SupportsSyntaxTree || !document.SupportsSemanticModel)
                return;

            SyntaxNode rootNode = await document.GetSyntaxRootAsync(CancellationToken.None).ConfigureAwait(false);
            SemanticModel semanticModel = await document.GetSemanticModelAsync(CancellationToken.None).ConfigureAwait(false);

            _analyzedSpans.RescanDocument(snapshot, rootNode, semanticModel, CancellationToken.None);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
        }

        IEnumerable<ITagSpan<IClassificationTag>> ITagger<IClassificationTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan snapshotSpan in spans)
            {
                ITextSnapshot snapshot = snapshotSpan.Snapshot;

                if (_buffer.CurrentSnapshot != snapshot)
                    yield break;

                foreach (AnalyzedSpan analyzedSpan in _analyzedSpans)
                {
                    foreach (SyntaxSpan syntaxSpan in analyzedSpan.GetSyntaxSpansIn(snapshotSpan))
                    {
                        Debug.Assert(snapshotSpan.Start <= syntaxSpan.End && syntaxSpan.Start <= snapshotSpan.End);

                        yield return _tagSpanFactory.CreateTagSpan(snapshot, in syntaxSpan);
                    }
                }
            }
        }

        IEnumerable<ITagSpan<IErrorTag>> ITagger<IErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan snapshotSpan in spans)
            {
                ITextSnapshot snapshot = snapshotSpan.Snapshot;

                if (_buffer.CurrentSnapshot != snapshot)
                    yield break;

                foreach (AnalyzedSpan analyzedSpan in _analyzedSpans)
                {
                    if (!analyzedSpan.TryGetDiagnosticSpanIn(snapshotSpan, out DiagnosticSpan diagnosticSpan))
                        continue;

                    yield return _tagSpanFactory.CreateTagSpan(snapshot, in diagnosticSpan);
                }
            }
        }
    }
}
