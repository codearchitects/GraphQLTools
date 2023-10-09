using GraphQLTools.Syntax;
using GraphQLTools.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphQLTools.Analysis
{
    internal readonly struct AnalyzedSpanBag
    {
        private readonly LinkedList<AnalyzedSpan> _analyzedSpans;
        private readonly ObjectPool<SyntaxSpanList> _spanListPool;
        private readonly object _synchronizationLock;

        public AnalyzedSpanBag(LinkedList<AnalyzedSpan> analyzedSpans, ObjectPool<SyntaxSpanList> spanListPool)
        {
            _analyzedSpans = analyzedSpans;
            _spanListPool = spanListPool;
            _synchronizationLock = new object();
        }

        public void Clear()
        {
            lock (_synchronizationLock)
            {
                foreach (AnalyzedSpan analyzedSpan in _analyzedSpans)
                {
                    _spanListPool.Return(analyzedSpan.ReturnSpanList());
                }

                _analyzedSpans.Clear();
            }
        }

        public Enumerator GetEnumerator()
        {
            bool lockTaken = false;
            Monitor.Enter(_synchronizationLock, ref lockTaken);

            return new Enumerator(_analyzedSpans, _synchronizationLock, lockTaken);
        }

        public void HandleChanges(INormalizedTextChangeCollection changes)
        {
            if (_analyzedSpans.Count == 0)
                return;

            lock (_synchronizationLock) // That shouldn't be necessary, but we keep it anyway.
            {
                LinkedListNode<AnalyzedSpan> node = _analyzedSpans.First;
                LinkedListNode<AnalyzedSpan> last = node.Previous;

                do
                {
                    LinkedListNode<AnalyzedSpan> current = node;
                    AnalyzedSpan analyzedSpan = current.Value;
                    node = node.Next;

                    analyzedSpan.HandleChanges(changes);
                    if (analyzedSpan.Length != 0)
                        continue;

                    analyzedSpan.ReturnSpanList();
                    _analyzedSpans.Remove(current);
                }
                while (node != last);
            }
        }

        public void RescanDocument(ITextSnapshot snapshot, SyntaxNode rootNode, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            Clear();
            Rescan(snapshot, new Span(0, snapshot.Length), rootNode, semanticModel, cancellationToken);
        }

        public void Rescan(ITextSnapshot snapshot, INormalizedTextChangeCollection changes, SyntaxNode rootNode, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (changes.Count == 1)
            {
                Rescan(snapshot, changes[0].NewSpan, rootNode, semanticModel, cancellationToken);
                return;
            }

            Rescan(snapshot, new Span(0, snapshot.Length), rootNode, semanticModel, cancellationToken);
        }

        private void Rescan(ITextSnapshot snapshot, Span span, SyntaxNode rootNode, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            IEnumerable<SyntaxNode> nodes = GetNodesToAnalyze(rootNode, span.ToRoslynSpan());

            foreach (SyntaxNode node in nodes)
            {
                InvocationExpressionSyntax invocation = node as InvocationExpressionSyntax;
                if (invocation is null)
                    continue;

                foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                {
                    if (!semanticModel.IsGqlString(argument))
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    ExpressionSyntax expression = argument.Expression;
                    int expressionSpanStart = expression.SpanStart;

                    if (TryGetAt(expressionSpanStart, out LinkedListNode<AnalyzedSpan> existingNode))
                    {
                        lock (_synchronizationLock)
                        {
                            _spanListPool.Return(existingNode.Value.ReturnSpanList());
                            _analyzedSpans.Remove(existingNode);
                        }
                    }

                    LiteralExpressionSyntax literalExpression = expression as LiteralExpressionSyntax;
                    if (literalExpression == null)
                        continue;

                    if (AnalyzedSpan.TryCreate(snapshot, literalExpression, out AnalyzedSpan analyzedSpan))
                    {
                        SyntaxSpanList spans = _spanListPool.Get();
                        analyzedSpan.Reparse(snapshot, invocation, literalExpression, ref spans);

                        lock (_synchronizationLock)
                        {
                            _analyzedSpans.AddLast(analyzedSpan);
                        }
                    }
                }
            }
        }

        private bool TryGetAt(int expressionSpanStart, out LinkedListNode<AnalyzedSpan> node)
        {
            node = _analyzedSpans.First;
            if (node == null)
                return false;

            LinkedListNode<AnalyzedSpan> last = node.Previous;

            do
            {
                if (node.Value.Start == expressionSpanStart)
                    return true;

                node = node.Next;
            }
            while (node != last);

            node = null;
            return false;
        }

        private static IEnumerable<SyntaxNode> GetNodesToAnalyze(SyntaxNode root, TextSpan span)
        {
            if (!span.IsEmpty)
                return root.DescendantNodes(span);

            int position = span.Start;
            if (position >= root.FullSpan.End)
                return Enumerable.Empty<SyntaxNode>();

            SyntaxNodeOrToken childAtPosition = root.ChildThatContainsPosition(span.Start);

            if (childAtPosition.IsNode)
                return childAtPosition.AsNode().DescendantNodes();

            return Enumerable.Empty<SyntaxNode>();
        }

        public static AnalyzedSpanBag Create(ObjectPool<SyntaxSpanList> spanListPool)
        {
            return new AnalyzedSpanBag(new LinkedList<AnalyzedSpan>(), spanListPool);
        }

        public struct Enumerator : IDisposable
        {
            private LinkedList<AnalyzedSpan>.Enumerator _enumerator;
            private readonly object _synchronizationLock;
            private readonly bool _lockTaken;

            public Enumerator(LinkedList<AnalyzedSpan> analyzedSpans, object synchronizationLock, bool lockTaken)
            {
                _enumerator = analyzedSpans.GetEnumerator();
                _synchronizationLock = synchronizationLock;
                _lockTaken = lockTaken;
            }

            public AnalyzedSpan Current => _enumerator.Current;

            public bool MoveNext() => _enumerator.MoveNext();

            public void Dispose()
            {
                if (_lockTaken)
                {
                    Monitor.Exit(_synchronizationLock);
                }
            }
        }
    }
}
