using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GraphQLTools.Syntax
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
            spans.Received().AddKeyword(new TextSpan(0, 5));
            spans.Received().AddOperationName(new TextSpan(6, 7));
            spans.Received().AddPunctuator(new TextSpan(14, 1));
            spans.Received().AddVariableName(new TextSpan(15, 4));
            spans.Received().AddPunctuator(new TextSpan(19, 1));
            spans.Received().AddPunctuator(new TextSpan(21, 1));
            spans.Received().AddTypeName(new TextSpan(22, 6));
            spans.Received().AddPunctuator(new TextSpan(28, 1));
            spans.Received().AddPunctuator(new TextSpan(29, 1));
            spans.Received().AddPunctuator(new TextSpan(30, 1));
            spans.Received().AddPunctuator(new TextSpan(31, 1));
            spans.Received().AddDirectiveName(new TextSpan(33, 4));
            spans.Received().AddPunctuator(new TextSpan(37, 1));
            spans.Received().AddArgumentName(new TextSpan(38, 3));
            spans.Received().AddPunctuator(new TextSpan(41, 1));
            spans.Received().AddNumber(new TextSpan(43, 2));
            spans.Received().AddPunctuator(new TextSpan(45, 1));
            spans.Received().AddPunctuator(new TextSpan(47, 1));
            spans.Received().AddFieldName(new TextSpan(49, 6));
            spans.Received().AddFieldName(new TextSpan(56, 5));
            spans.Received().AddPunctuator(new TextSpan(61, 1));
            spans.Received().AddAliasedFieldName(new TextSpan(63, 6));
            spans.Received().AddPunctuator(new TextSpan(69, 1));
            spans.Received().AddArgumentName(new TextSpan(70, 4));
            spans.Received().AddPunctuator(new TextSpan(74, 1));
            spans.Received().AddVariableName(new TextSpan(76, 4));
            spans.Received().AddPunctuator(new TextSpan(80, 1));
            spans.Received().AddArgumentName(new TextSpan(82, 4));
            spans.Received().AddPunctuator(new TextSpan(86, 1));
            spans.Received().AddEnum(new TextSpan(88, 4));
            spans.Received().AddPunctuator(new TextSpan(92, 1));
            spans.Received().AddDirectiveName(new TextSpan(94, 4));
            spans.Received().AddPunctuator(new TextSpan(98, 1));
            spans.Received().AddArgumentName(new TextSpan(99, 3));
            spans.Received().AddPunctuator(new TextSpan(102, 1));
            spans.Received().AddString(new TextSpan(104, 5));
            spans.Received().AddPunctuator(new TextSpan(109, 1));
            spans.Received().AddPunctuator(new TextSpan(111, 1));
            spans.Received().AddFieldName(new TextSpan(113, 8));
            spans.Received().AddPunctuator(new TextSpan(122, 1));
            spans.Received().AddPunctuator(new TextSpan(124, 1));
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
            spans.Received().AddKeyword(new TextSpan(0, 5));
            spans.Received().AddPunctuator(new TextSpan(6, 1));
            spans.Received().AddVariableName(new TextSpan(7, 4));
            spans.Received().AddPunctuator(new TextSpan(11, 1));
            spans.Received().AddError(new TextSpan(13, 1));
            spans.Received().SetDiagnostic(new TextSpan(13, 1), ErrorMessages.ExpectedType);
            spans.Received().AddPunctuator(new TextSpan(15, 1));
            spans.Received().AddName(new TextSpan(17, 5));
            spans.Received().AddPunctuator(new TextSpan(23, 1));
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
