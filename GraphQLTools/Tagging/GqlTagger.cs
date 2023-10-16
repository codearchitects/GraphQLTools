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
        private readonly TaggerCancellationTokenSource _cts;

        public GqlTagger(ITextBuffer2 buffer, GqlTagSpanFactory tagSpanFactory, ObjectPool<SyntaxSpanList> spanListPool)
        {
            _buffer = buffer;
            _tagSpanFactory = tagSpanFactory;
            _analyzedSpans = AnalyzedSpanBag.Create(spanListPool);
            _cts = TaggerCancellationTokenSource.Create();

            buffer.Changed += Buffer_Changed;
            buffer.ChangedOnBackground += Buffer_ChangedOnBackground;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Close()
        {
            _cts.Clear();
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
                _cts.Cancel(snapshot);
            }
        }

        private void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            _cts.Cancel(e.Before);
        }

        private void Buffer_ChangedOnBackground(object sender, TextContentChangedEventArgs e)
        {
            if (e.Changes.Count == 0)
                return;

            ITextSnapshot snapshot = e.After;
            INormalizedTextChangeCollection changes = e.Changes;
            CancellationToken cancellationToken = _cts.GetToken(snapshot);

            _analyzedSpans.Synchronize(changes);
            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                ThreadHelper.JoinableTaskFactory.Run(() => ScanChangesAsync(snapshot, changes, cancellationToken));
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _cts.Cancel(snapshot);
            }
        }

        private async Task ScanChangesAsync(ITextSnapshot snapshot, INormalizedTextChangeCollection changes, CancellationToken cancellationToken)
        {
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document is null || !document.SupportsSyntaxTree || !document.SupportsSemanticModel)
                return;

            SyntaxNode rootNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

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

                foreach (AnalyzedSpan analyzedSpan in _analyzedSpans)
                {
                    if (_buffer.CurrentSnapshot != snapshot)
                        yield break;
                    
                    foreach (SyntaxSpan syntaxSpan in analyzedSpan.GetSyntaxSpansIn(snapshotSpan))
                    {
                        Debug.Assert(snapshotSpan.Start <= syntaxSpan.End && syntaxSpan.Start <= snapshotSpan.End);

                        if (syntaxSpan.End > snapshot.Length)
                            continue;

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

                foreach (AnalyzedSpan analyzedSpan in _analyzedSpans)
                {
                    if (_buffer.CurrentSnapshot != snapshot)
                        yield break;
                    
                    if (!analyzedSpan.TryGetDiagnosticSpanIn(snapshotSpan, out DiagnosticSpan diagnosticSpan))
                        continue;

                    if (diagnosticSpan.End > snapshot.Length)
                        continue;

                    yield return _tagSpanFactory.CreateTagSpan(snapshot, in diagnosticSpan);
                }
            }
        }
    }
}
