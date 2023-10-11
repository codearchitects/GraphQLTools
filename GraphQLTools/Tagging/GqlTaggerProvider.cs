using GraphQLTools.Analysis;
using GraphQLTools.Classification;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using SyntaxSpanListPool = Microsoft.Extensions.ObjectPool.ObjectPool<GraphQLTools.Syntax.SyntaxSpanList>;

namespace GraphQLTools.Tagging
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("csharp")]
    [TagType(typeof(IClassificationTag)), TagType(typeof(IErrorTag))]
    internal sealed class GqlTaggerProvider : IViewTaggerProvider, IDisposable
    {
        private static readonly ObjectPoolProvider s_poolProvider = new DefaultObjectPoolProvider();

        private readonly VisualStudioWorkspace _workspace;
        private readonly GqlTagSpanFactory _tagSpanFactory;
        private readonly Dictionary<ITextView, GqlTagger> _openTaggers;
        private readonly SyntaxSpanListPool _spanListPool;

        [ImportingConstructor]
        public GqlTaggerProvider(VisualStudioWorkspace workspace, GqlClassificationTypes classificationTypes)
        {
            _workspace = workspace;
            _tagSpanFactory = new GqlTagSpanFactory(classificationTypes);
            _openTaggers = new Dictionary<ITextView, GqlTagger>();
            _spanListPool = s_poolProvider.Create(SyntaxSpanListPooledObjectPolicy.Instance);

            _workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer)
          where T : ITag
        {
            textView.Closed += TextView_Closed;

            return (ITagger<T>)buffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                GqlTagger tagger = new GqlTagger((ITextBuffer2)buffer, _tagSpanFactory, _spanListPool);
                _openTaggers.Add(textView, tagger);
                tagger.ScanDocument();
                return tagger;
            });
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            ITextView textView = (ITextView)sender;

            if (_openTaggers.TryGetValue(textView, out GqlTagger tagger))
            {
                tagger.Close();
                _openTaggers.Remove(textView);
            }

            textView.Closed -= TextView_Closed;
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            switch (e.Kind)
            {
                case WorkspaceChangeKind.ProjectChanged:
                    foreach (GqlTagger tagger in _openTaggers.Values)
                    {
                        tagger.ScanDocument(e.ProjectId);
                    }
                    break;
            }
        }

        public void Dispose()
        {
            _workspace.WorkspaceChanged -= Workspace_WorkspaceChanged;
        }
    }
}
