using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace GraphQLTools.Syntax;

internal ref struct GqlParser
{
    private static readonly string[] s_operationTypes = new string[] { "query", "mutation", "subscription" };
    private static readonly string[] s_literals = new string[] { "true", "false", "null" };
    private static readonly string[] s_onKeyword = new string[] { "on" };
    private static readonly string[] s_fragmentKeyword = new string[] { "fragment" };

    private readonly ISyntaxSpanCollection _spans;
    private GqlLexer _lexer;
    private TextSpan _anonymousOperationSpan;
    private bool _hasOperations;

    private GqlParser(ISyntaxSpanCollection spans, GqlLexer lexer)
    {
        _spans = spans;
        _lexer = lexer;
        _anonymousOperationSpan = default;
        _hasOperations = false;
    }

    private void Parse()
    {
        if (!_lexer.MoveNext())
            return;

        try
        {
            ParseDocument();
        }
        catch (DiagnosticException exception)
        {
            TextSpan span = _lexer.Span;
            if (span.IsEmpty)
            {
                span = new TextSpan(span.Start, 1);
            }
            else if (_lexer.Kind != TokenKind.EndOfFile)
            {
                _spans.AddError(span);
            }

            _spans.SetDiagnostic(span, exception.Message);
            Panic();
        }
        catch (AnonymousOperationException exception)
        {
            _spans.SetDiagnostic(_anonymousOperationSpan, exception.Message);
            Panic();
        }
        catch (BadSourceException)
        {
            Panic();
        }
    }

    private void Panic()
    {
        while (_lexer.MoveNext())
        {
            switch (_lexer.Kind)
            {
                case TokenKind.Error:
                    _spans.AddError(_lexer.Span);
                    break;

                case TokenKind.BadSource:
                    break;

                case TokenKind.Comma:
                case TokenKind.Bang:
                case TokenKind.QuestionMark:
                case TokenKind.Ampersand:
                case TokenKind.LeftParenthesis:
                case TokenKind.RightParenthesis:
                case TokenKind.LeftBracket:
                case TokenKind.RightBracket:
                case TokenKind.LeftBrace:
                case TokenKind.RightBrace:
                case TokenKind.Spread:
                case TokenKind.Colon:
                case TokenKind.Equals:
                case TokenKind.Pipe:
                    _spans.AddPunctuator(_lexer.Span);
                    break;

                case TokenKind.Name:
                    _spans.AddName(_lexer.Span);
                    break;

                case TokenKind.VariableName:
                    _spans.AddVariableName(_lexer.Span);
                    break;

                case TokenKind.DirectiveName:
                    _spans.AddDirectiveName(_lexer.Span);
                    break;

                case TokenKind.String:
                case TokenKind.BlockString:
                    _spans.AddString(_lexer.Span);
                    break;

                case TokenKind.Integer:
                case TokenKind.Float:
                    _spans.AddNumber(_lexer.Span);
                    break;

                case TokenKind.Comment:
                    _spans.AddComment(_lexer.Span);
                    break;
            }
        }
    }

    private void ParseDocument()
    {
        if (!ParseDefinition())
            throw new DiagnosticException(ErrorMessages.ExpectedDefinition);

        while (ParseDefinition());

        if (_lexer.Kind != TokenKind.EndOfFile)
            throw new DiagnosticException(ErrorMessages.ExpectedDefinition);
    }

    private bool ParseDefinition()
    {
        return ParseOperationDefinition() || ParseFragmentDefinition();
    }

    private bool ParseOperationDefinition()
    {
        if (ParseNamedOperationDefinition() || ParseAnonymousOperationDefinition())
        {
            _hasOperations = true;
            return true;
        }

        return false;
    }

    private bool ParseAnonymousOperationDefinition()
    {
        TextSpan currentSpan = _lexer.Span;

        if (ParseSelectionSet())
        {
            _anonymousOperationSpan = currentSpan;

            if (_hasOperations)
                throw new AnonymousOperationException();

            return true;
        }

        return false;
    }

    private bool ParseNamedOperationDefinition()
    {
        if (!ConsumeOperationType())
            return false;

        ConsumeOperationName();

        ParseVariableDefinitions();

        ParseDirectives();

        if (!ParseSelectionSet())
            throw new DiagnosticException(ErrorMessages.ExpectedSelectionSet);

        if (_anonymousOperationSpan != default)
            throw new AnonymousOperationException();

        return true;
    }

    private bool ParseSelectionSet()
    {
        if (!ConsumePunctuator(TokenKind.LeftBrace))
            return false;

        if (!ParseSelection())
            throw new DiagnosticException(ErrorMessages.ExpectedSelection);

        while (ParseSelection());

        if (!ConsumePunctuator(TokenKind.RightBrace))
            throw new DiagnosticException(ErrorMessages.ExpectedRightBrace);

        return true;
    }

    private bool ParseSelection()
    {
        return ParseField() || ParseFragmentSpreadOrInlineFragment();
    }

    private bool ParseField()
    {
        bool hasAlias = ConsumeFieldAlias();

        if (!ConsumeFieldName(hasAlias))
        {
            if (hasAlias)
                throw new DiagnosticException(ErrorMessages.ExpectedName);

            return false;
        }

        ParseArguments();

        ParseDirectives();

        ParseSelectionSet();

        return true;
    }

    private bool ParseDirectives()
    {
        if (!ParseDirective())
            return false;

        while (ParseDirective());

        return true;
    }

    private bool ParseDirective()
    {
        if (!ConsumeDirectiveName())
            return false;

        ParseArguments();

        return true;
    }

    private bool ParseFragmentSpreadOrInlineFragment()
    {
        if (!ConsumePunctuator(TokenKind.Spread))
            return false;

        if (!ParseFragmentSpread() && !ParseInlineFragment())
            throw new DiagnosticException(ErrorMessages.ExpectedFragment);

        return true;
    }

    private bool ParseFragmentSpread()
    {
        if (!ConsumeFragmentName())
            return false;

        ParseDirectives();

        return true;
    }

    private bool ParseInlineFragment()
    {
        ParseTypeCondition();

        ParseDirectives();

        if (!ParseSelectionSet())
            throw new DiagnosticException(ErrorMessages.ExpectedSelectionSet);

        return true;
    }

    private bool ParseTypeCondition()
    {
        if (!ConsumeOnKeyword())
            return false;

        if (!ConsumeNamedType())
            throw new DiagnosticException(ErrorMessages.ExpectedTypeName);

        return true;
    }

    private bool ParseArguments()
    {
        if (!ConsumePunctuator(TokenKind.LeftParenthesis))
            return false;

        if (!ParseArgument())
            throw new DiagnosticException(ErrorMessages.ExpectedArgument);

        while (ParseArgument());

        if (!ConsumePunctuator(TokenKind.RightParenthesis))
            throw new DiagnosticException(ErrorMessages.ExpectedRightParenthesis);

        return true;
    }

    private bool ParseArgument()
    {
        if (!ConsumeArgumentName())
            return false;

        if (!ConsumePunctuator(TokenKind.Colon))
            throw new DiagnosticException(ErrorMessages.ExpectedColon);

        if (!ParseValue())
            throw new DiagnosticException(ErrorMessages.ExpectedValue);

        return true;
    }

    private bool ParseValue()
    {
        return
          ConsumeVariableName() ||
          ConsumeNumber()       ||
          ConsumeString()       ||
          ConsumeLiteral()      ||
          ConsumeEnum()         ||
          ParseListValue()      ||
          ParseObjectValue();
    }

    private bool ParseObjectValue()
    {
        if (!ConsumePunctuator(TokenKind.LeftBrace))
            return false;

        while (ParseObjectField());

        if (!ConsumePunctuator(TokenKind.RightBrace))
            throw new DiagnosticException(ErrorMessages.ExpectedRightBrace);

        return true;
    }

    private bool ParseObjectField()
    {
        if (!ConsumeObjectFieldName())
            return false;

        if (!ConsumePunctuator(TokenKind.Colon))
            throw new DiagnosticException(ErrorMessages.ExpectedColon);

        if (!ParseValue())
            throw new DiagnosticException(ErrorMessages.ExpectedValue);

        return true;
    }

    private bool ParseListValue()
    {
        if (!ConsumePunctuator(TokenKind.LeftBracket))
            return false;

        while (ParseValue());

        if (!ConsumePunctuator(TokenKind.RightBracket))
            throw new DiagnosticException(ErrorMessages.ExpectedRightBracket);

        return true;
    }

    private bool ParseVariableDefinitions()
    {
        if (!ConsumePunctuator(TokenKind.LeftParenthesis))
            return false;

        if (!ParseVariableDefinition())
            throw new DiagnosticException(ErrorMessages.ExpectedVariableDefinition);

        while (ParseVariableDefinition());

        if (!ConsumePunctuator(TokenKind.RightParenthesis))
            throw new DiagnosticException(ErrorMessages.ExpectedRightParenthesis);

        return true;
    }

    private bool ParseVariableDefinition()
    {
        if (!ConsumeVariableName())
            return false;

        if (!ConsumePunctuator(TokenKind.Colon))
            throw new DiagnosticException(ErrorMessages.ExpectedColon);

        if (!ParseType())
            throw new DiagnosticException(ErrorMessages.ExpectedType);

        ParseDefaultValue();

        return true;
    }

    private bool ParseType()
    {
        return ParseNamedOrNotNullType() || ParseListOrNotNullType();
    }

    private bool ParseNamedOrNotNullType()
    {
        if (!ConsumeNamedType())
            return false;

        ConsumePunctuator(TokenKind.Bang);
        return true;
    }

    private bool ParseListOrNotNullType()
    {
        if (!ConsumePunctuator(TokenKind.LeftBracket))
            return false;

        if (!ParseType())
            throw new DiagnosticException(ErrorMessages.ExpectedType);

        if (!ConsumePunctuator(TokenKind.RightBracket))
            throw new DiagnosticException(ErrorMessages.ExpectedRightBracket);

        ConsumePunctuator(TokenKind.Bang);

        return true;
    }

    private bool ParseDefaultValue()
    {
        if (!ConsumePunctuator(TokenKind.Equals))
            return false;

        if (!ParseValue())
            throw new DiagnosticException(ErrorMessages.ExpectedValue);

        return true;
    }

    private bool ParseFragmentDefinition()
    {
        if (!ConsumeFragmentKeyword())
            return false;

        if (!ConsumeFragmentName())
            throw new DiagnosticException(ErrorMessages.ExpectedFragmentName);

        if (!ParseTypeCondition())
            throw new DiagnosticException(ErrorMessages.ExpectedTypeCondition);

        ParseDirectives();

        if (!ParseSelectionSet())
            throw new DiagnosticException(ErrorMessages.ExpectedSelectionSet);

        return true;
    }

    private bool ConsumeOperationType()
    {
        if (!Match(TokenKind.Name, s_operationTypes))
            return false;

        _spans.AddKeyword(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeOperationName()
    {
        if (!Match(TokenKind.Name))
            return false;

        _spans.AddOperationName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeFragmentName()
    {
        if (!MatchButNot(TokenKind.Name, s_onKeyword))
            return false;

        _spans.AddFragmentName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeNamedType()
    {
        if (!Match(TokenKind.Name))
            return false;

        _spans.AddTypeName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeFieldAlias()
    {
        if (_lexer.Lookahead() != TokenKind.Colon)
            return false;

        if (!Match(TokenKind.Name))
            return false;

        _spans.AddFieldName(_lexer.Span);
        MoveNext();
        Debug.Assert(_lexer.Kind == TokenKind.Colon);

        _spans.AddPunctuator(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeFieldName(bool hasAlias)
    {
        if (!Match(TokenKind.Name))
            return false;

        if (hasAlias)
        {
            _spans.AddAliasedFieldName(_lexer.Span);
        }
        else
        {
            _spans.AddFieldName(_lexer.Span);
        }
        MoveNext();

        return true;
    }

    private bool ConsumeObjectFieldName()
    {
        if (!Match(TokenKind.Name))
            return false;

        _spans.AddObjectFieldName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeVariableName()
    {
        if (!Match(TokenKind.VariableName))
            return false;

        _spans.AddVariableName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeDirectiveName()
    {
        if (!Match(TokenKind.DirectiveName))
            return false;

        _spans.AddDirectiveName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeArgumentName()
    {
        if (!Match(TokenKind.Name))
            return false;

        _spans.AddArgumentName(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeString()
    {
        if (!Match(TokenKind.String) && !Match(TokenKind.BlockString))
            return false;

        _spans.AddString(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeNumber()
    {
        if (!Match(TokenKind.Integer) && !Match(TokenKind.Float))
            return false;

        _spans.AddNumber(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeOnKeyword()
    {
        if (!Match(TokenKind.Name, s_onKeyword))
            return false;

        _spans.AddKeyword(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeFragmentKeyword()
    {
        if (!Match(TokenKind.Name, s_fragmentKeyword))
            return false;

        _spans.AddKeyword(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeEnum()
    {
        if (!Match(TokenKind.Name))
            return false;

        _spans.AddEnum(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumeLiteral()
    {
        if (!Match(TokenKind.Name, s_literals))
            return false;

        _spans.AddKeyword(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool ConsumePunctuator(TokenKind kind)
    {
        if (!Match(kind))
            return false;

        _spans.AddPunctuator(_lexer.Span);
        MoveNext();

        return true;
    }

    private bool Match(TokenKind kind)
    {
        return _lexer.Kind == kind;
    }

    private readonly bool Match(TokenKind kind, string[] values)
    {
        if (_lexer.Kind != kind)
            return false;

        foreach (string value in values)
        {
            if (_lexer.ValueMatch(value))
                return true;
        }

        return false;
    }

    private readonly bool MatchButNot(TokenKind kind, string[] values)
    {
        if (_lexer.Kind != kind)
            return false;

        foreach (string value in values)
        {
            if (_lexer.ValueMatch(value))
                return false;
        }

        return true;
    }

    private void MoveNext()
    {
        while (_lexer.MoveNext())
        {
            switch (_lexer.Kind)
            {
                case TokenKind.Comment:
                    _spans.AddComment(_lexer.Span);
                    break;

                case TokenKind.Comma:
                    _spans.AddPunctuator(_lexer.Span);
                    break;

                case TokenKind.Error:
                    throw new DiagnosticException(_lexer.ErrorMessage ?? string.Empty);

                case TokenKind.BadSource:
                    throw new BadSourceException();

                default:
                    return;
            }
        }
    }

    public static void Parse(SourceText source, ISyntaxSpanCollection spans)
    {
        new GqlParser(spans, new GqlLexer(source)).Parse();
    }

    private sealed class DiagnosticException : Exception
    {
        public DiagnosticException(string message)
          : base(message)
        {
        }
    }

    private sealed class BadSourceException : Exception
    {
    }

    private sealed class AnonymousOperationException : Exception
    {
        public AnonymousOperationException()
          : base(ErrorMessages.AnonymousOperationError)
        {
        }
    }
}
