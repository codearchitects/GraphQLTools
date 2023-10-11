using Microsoft.VisualStudio.Text;
using System.Runtime.CompilerServices;
using static GraphQLTools.Syntax.TextFacts;

namespace GraphQLTools.Syntax
{
    internal ref struct GqlLexer
    {
        private readonly SourceText _source;
        private int _spanStart;

        public GqlLexer(SourceText source)
        {
            _source = source;
            _spanStart = source.Position;
            Kind = TokenKind.StartOfFile;
            ErrorMessage = null;
        }

        public TokenKind Kind { get; private set; }

        public string ErrorMessage { get; private set; }

        public Span Span => new Span(_spanStart, Length);

        private int Length => _source.Position - _spanStart;

        public TokenKind Lookahead()
        {
            GqlLexer lexer = this;
            var checkpoint = _source.Checkpoint();

            try
            {
                lexer.MoveNext();
                return lexer.Kind;
            }
            finally
            {
                checkpoint.Reset();
            }
        }

        public bool ValueMatch(string expected)
        {
            if (Length != expected.Length)
                return false;

            return _source.Match(expected);
        }

        public bool MoveNext()
        {
            if (Kind == TokenKind.EndOfFile)
                return false;

            _spanStart = _source.Position;
            ErrorMessage = null;

            if (!_source.MoveNext())
            {
                Kind = TokenKind.EndOfFile;
                return true;
            }

            char current = _source.Current;

            if (IsAlpha(current))
                return Name();

            if (current == '{')
                return Token(TokenKind.LeftBrace);

            if (current == '}')
                return Token(TokenKind.RightBrace);

            if (current == '"')
                return _source.Eat("\"\"") ? BlockString() : String();

            if (current == ' ' || current == '\t' || current == '\n' || current == '\r')
                return Ignored();

            if (current == ',')
                return Token(TokenKind.Comma);

            if (current >= '1' && current <= '9')
                return NonZeroDigit();

            if (current == '-')
                return MinusSign();

            if (current == '0')
                return Zero();

            if (current == '(')
                return Token(TokenKind.LeftParenthesis);

            if (current == ')')
                return Token(TokenKind.RightParenthesis);

            if (current == '[')
                return Token(TokenKind.LeftBracket);

            if (current == ']')
                return Token(TokenKind.RightBracket);

            if (current == ':')
                return Token(TokenKind.Colon);

            if (current == '=')
                return Token(TokenKind.Equals);

            if (current == '$')
                return VariableName();

            if (current == '@')
                return DirectiveName();

            if (current == '.' && _source.Eat(".."))
                return Token(TokenKind.Spread);

            if (current == '|')
                return Token(TokenKind.Pipe);

            if (current == '!')
                return Token(TokenKind.Bang);

            if (current == '?')
                return Token(TokenKind.QuestionMark);

            if (current == '&')
                return Token(TokenKind.Ampersand);

            if (current == '#')
                return Comment();

            if (current == SourceText.InvalidChar)
                return Token(TokenKind.BadSource);

            return Unexpected(current);
        }

        private bool Name()
        {
            _source.EatWhile(IsAlphaNumeric);

            return Token(TokenKind.Name);
        }

        private bool VariableName()
        {
            if (!_source.Eat(IsAlpha))
            {
                _source.EatWhile(IsWordLike);
                return Error(ErrorMessages.ExpectedName);
            }

            _source.EatWhile(IsAlphaNumeric);

            return Token(TokenKind.VariableName);
        }

        private bool DirectiveName()
        {
            if (!_source.Eat(IsAlpha))
            {
                _source.EatWhile(IsWordLike);
                return Error(ErrorMessages.ExpectedName);
            }

            _source.EatWhile(IsAlphaNumeric);

            return Token(TokenKind.DirectiveName);
        }

        private bool String()
        {
            bool isValid = true;

            while (_source.MoveNext())
            {
                char current = _source.Current;

                if (IsNewlineCharacter(current))
                {
                    return Error(ErrorMessages.UnterminatedString);
                }

                if (current == '"')
                {
                    return isValid
                        ? Token(TokenKind.String)
                        : Error(ErrorMessages.InvalidString);
                }

                if (current == '\\')
                {
                    isValid &= EatEscapeSequence();
                }
            }

            return Error(ErrorMessages.UnterminatedString);
        }

        private bool EatEscapeSequence()
        {
            if (_source.Eat('u'))
                return EatUnicodeSequence();

            return _source.Eat(IsEscaped);
        }

        private bool EatUnicodeSequence()
        {
            for (int i = 0; i < 4; i++)
            {
                if (!_source.Eat(IsHexDigit))
                    return false;
            }

            return true;
        }

        private bool BlockString()
        {
            while (_source.MoveNext())
            {
                char current = _source.Current;

                if (current == '"' && _source.Eat("\"\""))
                    return Token(TokenKind.BlockString);

                if (current == '\\')
                {
                    _source.Eat("\"\"");
                }
            }

            return Error(ErrorMessages.UnterminatedString);
        }

        private bool NonZeroDigit()
        {
            _source.EatWhile(IsDigit);

            TokenKind kind = CheckFloat();

            if (_source.EatWhile(IsWordLike))
                return Error(ErrorMessages.InvalidNumber);

            return kind == TokenKind.Error
              ? Error(ErrorMessages.InvalidNumber)
              : Token(kind);
        }

        private bool MinusSign()
        {
            while (_source.MoveNext())
            {
                char current = _source.Current;

                if (current == '0')
                    return Zero();

                if (IsDigit(current))
                    return NonZeroDigit();

                _source.EatWhile(IsWordLike);
            }

            return Error(ErrorMessages.InvalidNumber);
        }

        private bool Zero()
        {
            TokenKind kind = CheckFloat();

            if (kind == TokenKind.Integer && _source.EatWhile(IsDigit))
                return Error(ErrorMessages.InvalidNumber);

            if (_source.EatWhile(IsWordLike))
                return Error(ErrorMessages.InvalidNumber);

            return kind == TokenKind.Error
              ? Error(ErrorMessages.InvalidNumber)
              : Token(kind);
        }

        private bool Ignored()
        {
            _source.EatWhile(IsIgnored);

            return MoveNext();
        }

        private bool Comment()
        {
            _source.EatUntil(IsNewlineCharacter);

            return Token(TokenKind.Comment);
        }

        private bool Unexpected(char current)
        {
            return Error(ErrorMessages.UnexpectedCharacter(current));
        }

        private TokenKind CheckFloat()
        {
            TokenKind kind = TokenKind.Integer;

            if (_source.Eat('.'))
            {
                kind = TokenKind.Float;

                if (!_source.EatWhile(IsDigit))
                    return TokenKind.Error;
            }

            if (_source.Eat(IsExponentIndicator))
            {
                kind = TokenKind.Float;

                _source.Eat(IsSign);

                if (!_source.EatWhile(IsDigit))
                    return TokenKind.Error;
            }

            return kind;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Token(TokenKind kind)
        {
            Kind = kind;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Error(string message)
        {
            Kind = TokenKind.Error;
            ErrorMessage = message;
            return true;
        }
    }
}
