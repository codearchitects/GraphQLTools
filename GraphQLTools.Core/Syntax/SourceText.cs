using System;
using System.Diagnostics;

namespace GraphQLTools.Syntax;

internal abstract class SourceText
{
    public const char InvalidChar = '\uFFFF';

    protected SourceText(int position)
    {
        Position = position;
    }

    public int Position { get; private set; }

    public char Current { get; private set; }

    protected abstract int Start { get; }

    protected abstract int End { get; }

    protected abstract char MoveNext(ref int position);

    public LookaheadCheckpoint Checkpoint() => new LookaheadCheckpoint(this);

    public bool MoveNext()
    {
        int position = Position;
        if (position == End)
            return false;

        char next = MoveNext(ref position);

        Position = position;
        Current = next;
        return true;
    }

    public bool Match(string expected)
    {
        Debug.Assert(expected.Length > 0);

        int position = Position - expected.Length;
        if (position < Start)
            return false;

        char next = MoveNext(ref position);
        if (next != expected[0])
            return false;

        for (int i = 1; i < expected.Length; i++)
        {
            if (position == End)
                return false;

            next = MoveNext(ref position);

            if (next != expected[i])
                return false;
        }

        return true;
    }

    public bool Eat(char expected)
    {
        int position = Position;
        if (position == End)
            return false;

        char next = MoveNext(ref position);

        if (next != expected)
            return false;

        Position = position;
        Current = next;
        return true;
    }

    public bool Eat(string expected)
    {
        Debug.Assert(expected.Length > 0);

        int position = Position;
        if (position == End)
            return false;

        char next = MoveNext(ref position);
        if (next != expected[0])
            return false;

        for (int i = 1; i < expected.Length; i++)
        {
            if (position == End)
                return false;

            next = MoveNext(ref position);

            if (next != expected[i])
                return false;
        }

        Position = position;
        Current = next;
        return true;
    }

    public bool Eat(Predicate<char> predicate)
    {
        int position = Position;
        if (position == End)
            return false;

        char next = MoveNext(ref position);

        if (!predicate(next))
            return false;

        Position = position;
        Current = next;
        return true;
    }

    public bool EatWhile(Predicate<char> predicate)
    {
        int position = Position;
        if (position == End)
            return false;

        char next = MoveNext(ref position);

        if (!predicate(next))
            return false;

        do
        {
            Position = position;
            Current = next;

            if (position == End)
                return true;

            next = MoveNext(ref position);
        }
        while (predicate(next));

        return true;
    }

    public bool EatUntil(Predicate<char> predicate)
    {
        int position = Position;
        if (position == End)
            return false;

        char next = MoveNext(ref position);

        if (predicate(next))
            return false;

        do
        {
            Position = position;
            Current = next;

            if (position == End)
                return true;

            next = MoveNext(ref position);
        }
        while (!predicate(next));

        return true;
    }

    public readonly ref struct LookaheadCheckpoint
    {
        private readonly SourceText _source;
        private readonly int _position;

        public LookaheadCheckpoint(SourceText source)
        {
            _source = source;
            _position = source.Position;
        }

        public void Reset()
        {
            _source.Position = _position;
        }
    }
}