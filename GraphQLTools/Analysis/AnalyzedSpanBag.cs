using GraphQLTools.Utils;
using Microsoft.CodeAnalysis;
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

        public void Synchronize(INormalizedTextChangeCollection changes)
        {
            if (_analyzedSpans.Count == 0)
                return;

            lock (_synchronizationLock)
            {
                foreach (AnalyzedSpan analyzedSpan in _analyzedSpans)
                {
                    analyzedSpan.Synchronize(changes);
                }
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
            RemoveInvalidatedNodes();

            IEnumerable<SyntaxNode> nodes = GetNodesToAnalyze(rootNode, new TextSpan(span.Start, span.Length));

            foreach (SyntaxNode node in nodes)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                InvocationExpressionSyntax invocation = node as InvocationExpressionSyntax;
                if (invocation is null)
                    continue;

                foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                {
                    if (!semanticModel.IsGqlString(argument))
                        continue;

                    ScanGqlExpression(snapshot, invocation, argument.Expression);
                }
            }
        }

        private void ScanGqlExpression(ITextSnapshot snapshot, InvocationExpressionSyntax invocation, ExpressionSyntax expression)
        {
            int expressionSpanStart = expression.SpanStart;
            LiteralExpressionSyntax literalExpression = expression as LiteralExpressionSyntax;

            if (TryGetAt(expressionSpanStart, out LinkedListNode<AnalyzedSpan> existingNode))
            {
                ScanExistingGqlExpression(snapshot, invocation, literalExpression, existingNode);
                return;
            }

            if (literalExpression == null || !AnalyzedSpan.TryCreate(snapshot, literalExpression, out AnalyzedSpan analyzedSpan))
                return;

            SyntaxSpanList spans = _spanListPool.Get();
            analyzedSpan.Reparse(snapshot, invocation, literalExpression, ref spans);

            lock (_synchronizationLock)
            {
                _analyzedSpans.AddLast(analyzedSpan);
            }
        }

        private void ScanExistingGqlExpression(ITextSnapshot snapshot, InvocationExpressionSyntax invocation, LiteralExpressionSyntax literalExpression, LinkedListNode<AnalyzedSpan> existingNode)
        {
            SyntaxSpanList spans;
            AnalyzedSpan existingAnalyzedSpan = existingNode.Value;

            if (!existingAnalyzedSpan.IsUnterminated && literalExpression != null)
            {
                spans = _spanListPool.Get();
                existingAnalyzedSpan.Reparse(snapshot, invocation, literalExpression, ref spans);
                _spanListPool.Return(spans);
                return;
            }

            if (literalExpression == null || !AnalyzedSpan.TryCreate(snapshot, literalExpression, out AnalyzedSpan analyzedSpan))
            {
                lock (_synchronizationLock)
                {
                    RemoveNode(existingNode);
                }

                return;
            }

            spans = _spanListPool.Get();
            analyzedSpan.Reparse(snapshot, invocation, literalExpression, ref spans);

            lock (_synchronizationLock)
            {
                RemoveNode(existingNode);
                _analyzedSpans.AddLast(analyzedSpan);
            }
        }

        private void RemoveNode(LinkedListNode<AnalyzedSpan> existingNode)
        {
            _spanListPool.Return(existingNode.Value.ReturnSpanList());
            _analyzedSpans.Remove(existingNode);
        }

        private void RemoveInvalidatedNodes()
        {
            if (_analyzedSpans.Count == 0)
                return;

            lock (_synchronizationLock)
            {
                LinkedListNode<AnalyzedSpan> node = _analyzedSpans.First;
                LinkedListNode<AnalyzedSpan> last = node.Previous;

                do
                {
                    LinkedListNode<AnalyzedSpan> current = node;
                    node = node.Next;

                    if (current.Value.Length != 0)
                        continue;

                    RemoveNode(current);
                }
                while (node != last);
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
