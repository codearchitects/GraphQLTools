using GraphQLTools.Syntax;
using Microsoft.VisualStudio.Text;
using System;
using System.Diagnostics;
using System.Threading;

namespace GraphQLTools.Analysis
{
    internal readonly struct SyntaxSpanListSlice
    {
        private readonly SyntaxSpanList _list;
        private readonly Span _span;

        public SyntaxSpanListSlice(SyntaxSpanList list, Span span)
        {
            _list = list;
            _span = span;
        }

        public Enumerator GetEnumerator()
        {
            if (!Monitor.TryEnter(_list, 100))
                return new Enumerator(SyntaxSpanList.EmptySlice._list, false, 0, 0);

            int startIndex = FindStartIndex();
            return new Enumerator(_list, true, startIndex, _span.End);
        }

        private int FindStartIndex()
        {
            int position = _span.Start;
            int left = 0;
            int right = _list.Count - 1;
            int index = _list.Count;

            while (left <= right)
            {
                int mid = (right + left) / 2;

                if (_list[mid].End >= position)
                {
                    index = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return index;
        }

        public struct Enumerator : IDisposable
        {
            private readonly SyntaxSpanList _spans;
            private readonly bool _lockTaken;
            private readonly int _endPosition;
            private int _index;

            public Enumerator(SyntaxSpanList spans, bool lockTaken, int startIndex, int endPosition)
            {
                _spans = spans;
                _lockTaken = lockTaken;
                _endPosition = endPosition;
                _index = startIndex - 1;
            }

            public SyntaxSpan Current
            {
                get
                {
                    Debug.Assert(_index >= 0 && _index < _spans.Count);

                    return _spans[_index];
                }
            }

            public void Dispose()
            {
                if (_lockTaken)
                {
                    Monitor.Exit(_spans);
                }
            }

            public bool MoveNext()
            {
                if (_index == _spans.Count - 1)
                    return false;

                _index++;

                if (_spans[_index].Start >= _endPosition)
                {
                    _index = _spans.Count;
                    return false;
                }

                return true;
            }
        }
    }
}
