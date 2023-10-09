using GraphQLTools.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace GraphQLTools.Tests.Syntax
{
    [TestClass]
    public class GqlParserTests
    {
        [TestMethod]
        public void Parse_ShouldAddNoDiagnostics_WhenDocumentIsWellFormed()
        {
            // Arrange
            ISyntaxSpanCollection spans = Substitute.For<ISyntaxSpanCollection>();
            var source = new StringSourceText("query MyQuery ($var: [String!]!) @dir(arg: 12) { field1 alias: field2(arg1: $var, arg2: ENUM) @dir(arg: \"str\") { subfield } }");

            // Act
            GqlParser.Parse(source, spans);

            // Assert
            spans.Received().AddKeyword(new Span(0, 5));
            spans.Received().AddOperationName(new Span(6, 7));
            spans.Received().AddPunctuator(new Span(14, 1));
            spans.Received().AddVariableName(new Span(15, 4));
            spans.Received().AddPunctuator(new Span(19, 1));
            spans.Received().AddPunctuator(new Span(21, 1));
            spans.Received().AddTypeName(new Span(22, 6));
            spans.Received().AddPunctuator(new Span(28, 1));
            spans.Received().AddPunctuator(new Span(29, 1));
            spans.Received().AddPunctuator(new Span(30, 1));
            spans.Received().AddPunctuator(new Span(31, 1));
            spans.Received().AddDirectiveName(new Span(33, 4));
            spans.Received().AddPunctuator(new Span(37, 1));
            spans.Received().AddArgumentName(new Span(38, 3));
            spans.Received().AddPunctuator(new Span(41, 1));
            spans.Received().AddNumber(new Span(43, 2));
            spans.Received().AddPunctuator(new Span(45, 1));
            spans.Received().AddPunctuator(new Span(47, 1));
            spans.Received().AddFieldName(new Span(49, 6));
            spans.Received().AddFieldName(new Span(56, 5));
            spans.Received().AddPunctuator(new Span(61, 1));
            spans.Received().AddAliasedFieldName(new Span(63, 6));
            spans.Received().AddPunctuator(new Span(69, 1));
            spans.Received().AddArgumentName(new Span(70, 4));
            spans.Received().AddPunctuator(new Span(74, 1));
            spans.Received().AddVariableName(new Span(76, 4));
            spans.Received().AddPunctuator(new Span(80, 1));
            spans.Received().AddArgumentName(new Span(82, 4));
            spans.Received().AddPunctuator(new Span(86, 1));
            spans.Received().AddEnum(new Span(88, 4));
            spans.Received().AddPunctuator(new Span(92, 1));
            spans.Received().AddDirectiveName(new Span(94, 4));
            spans.Received().AddPunctuator(new Span(98, 1));
            spans.Received().AddArgumentName(new Span(99, 3));
            spans.Received().AddPunctuator(new Span(102, 1));
            spans.Received().AddString(new Span(104, 5));
            spans.Received().AddPunctuator(new Span(109, 1));
            spans.Received().AddPunctuator(new Span(111, 1));
            spans.Received().AddFieldName(new Span(113, 8));
            spans.Received().AddPunctuator(new Span(122, 1));
            spans.Received().AddPunctuator(new Span(124, 1));
        }

        [TestMethod]
        public void Parse_ShouldAddDiagnostics_WhenDocumentHasSyntaxErrors()
        {
            // Arrange
            ISyntaxSpanCollection spans = Substitute.For<ISyntaxSpanCollection>();
            var source = new StringSourceText("query ($var: ) { field }");

            // Act
            GqlParser.Parse(source, spans);

            // Assert
            spans.Received().AddKeyword(new Span(0, 5));
            spans.Received().AddPunctuator(new Span(6, 1));
            spans.Received().AddVariableName(new Span(7, 4));
            spans.Received().AddPunctuator(new Span(11, 1));
            spans.Received().AddError(new Span(13, 1));
            spans.Received().SetDiagnostic(new Span(13, 1), ErrorMessages.ExpectedType);
            spans.Received().AddPunctuator(new Span(15, 1));
            spans.Received().AddName(new Span(17, 5));
            spans.Received().AddPunctuator(new Span(23, 1));
        }

        private class StringSourceText : SourceText
        {
            private readonly string _source;

            public StringSourceText(string source)
              : base(0)
            {
                _source = source;
            }

            protected override int Start => 0;

            protected override int End => _source.Length;

            protected override char MoveNext(ref int position)
            {
                return _source[position++];
            }
        }
    }
}
