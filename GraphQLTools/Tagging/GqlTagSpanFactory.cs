using GraphQLTools.Classification;
using GraphQLTools.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System;

namespace GraphQLTools.Tagging
{
    internal sealed class GqlTagSpanFactory
    {
        private readonly ClassificationTag _punctuator;
        private readonly ClassificationTag _keyword;
        private readonly ClassificationTag _operationName;
        private readonly ClassificationTag _fragmentName;
        private readonly ClassificationTag _variableName;
        private readonly ClassificationTag _directiveName;
        private readonly ClassificationTag _typeName;
        private readonly ClassificationTag _fieldName;
        private readonly ClassificationTag _aliasedFieldName;
        private readonly ClassificationTag _argumentName;
        private readonly ClassificationTag _objectFieldName;
        private readonly ClassificationTag _string;
        private readonly ClassificationTag _number;
        private readonly ClassificationTag _enum;
        private readonly ClassificationTag _name;
        private readonly ClassificationTag _comment;
        private readonly ClassificationTag _error;

        public GqlTagSpanFactory(GqlClassificationTypes classificationTypes)
        {
            _punctuator       = new ClassificationTag(classificationTypes.Punctuator);
            _keyword          = new ClassificationTag(classificationTypes.Keyword);
            _operationName    = new ClassificationTag(classificationTypes.OperationName);
            _fragmentName     = new ClassificationTag(classificationTypes.FragmentName);
            _variableName     = new ClassificationTag(classificationTypes.VariableName);
            _directiveName    = new ClassificationTag(classificationTypes.DirectiveName);
            _typeName         = new ClassificationTag(classificationTypes.TypeName);
            _fieldName        = new ClassificationTag(classificationTypes.FieldName);
            _aliasedFieldName = new ClassificationTag(classificationTypes.AliasedFieldName);
            _argumentName     = new ClassificationTag(classificationTypes.ArgumentName);
            _objectFieldName  = new ClassificationTag(classificationTypes.ObjectFieldName);
            _string           = new ClassificationTag(classificationTypes.String);
            _number           = new ClassificationTag(classificationTypes.Number);
            _enum             = new ClassificationTag(classificationTypes.Enum);
            _name             = new ClassificationTag(classificationTypes.Name);
            _comment          = new ClassificationTag(classificationTypes.Comment);
            _error            = new ClassificationTag(classificationTypes.Error);
        }

        public ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, in SyntaxSpan span)
        {
            IClassificationTag tag = GetClassificationTag(span.Kind);
            return new TagSpan<IClassificationTag>(new SnapshotSpan(snapshot, new Span(span.Start, span.Length)), tag);
        }

        public ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, in DiagnosticSpan span)
        {
            ErrorTag tag = new ErrorTag(PredefinedErrorTypeNames.OtherError, span.Message);
            return new TagSpan<IErrorTag>(new SnapshotSpan(snapshot, new Span(span.Start, span.Length)), tag);
        }

        private IClassificationTag GetClassificationTag(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.Punctuator:
                    return _punctuator;

                case SyntaxKind.Keyword:
                    return _keyword;

                case SyntaxKind.OperationName:
                    return _operationName;

                case SyntaxKind.FragmentName:
                    return _fragmentName;

                case SyntaxKind.VariableName:
                    return _variableName;

                case SyntaxKind.DirectiveName:
                    return _directiveName;

                case SyntaxKind.TypeName:
                    return _typeName;

                case SyntaxKind.FieldName:
                    return _fieldName;

                case SyntaxKind.AliasedFieldName:
                    return _aliasedFieldName;

                case SyntaxKind.ArgumentName:
                    return _argumentName;

                case SyntaxKind.ObjectFieldName:
                    return _objectFieldName;

                case SyntaxKind.String:
                    return _string;

                case SyntaxKind.Number:
                    return _number;

                case SyntaxKind.Enum:
                    return _enum;

                case SyntaxKind.Name:
                    return _name;

                case SyntaxKind.Comment:
                    return _comment;

                case SyntaxKind.Error:
                    return _error;

                default:
                    throw new ArgumentException($"Invalid {nameof(SyntaxKind)}.", nameof(kind));
            }
        }
    }
}
