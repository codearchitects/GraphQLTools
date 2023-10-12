namespace GraphQLTools.Syntax;

internal enum TokenKind
{
    EndOfFile,        // <EOF>
    StartOfFile,      // <SOF>
    Error,            // N/D
    BadSource,        // N/D
    Comma,            // ,
    Bang,             // !
    QuestionMark,     // ?
    Ampersand,        // &
    LeftParenthesis,  // (
    RightParenthesis, // )
    LeftBracket,      // [
    RightBracket,     // ]
    LeftBrace,        // {
    RightBrace,       // }
    Spread,           // ...
    Colon,            // :
    Equals,           // =
    Pipe,             // |
    Name,             // name
    VariableName,     // $name
    DirectiveName,    // @name
    String,           // "string"
    BlockString,      // """line1 \n line2"""
    Integer,          // 42
    Float,            // 69.420
    Comment           // # comment
}
