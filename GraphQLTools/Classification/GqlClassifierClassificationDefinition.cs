using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace GraphQLTools.Classification
{
    internal static class GqlClassifierClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlPunctuatorFormat.Name)]
        public static ClassificationTypeDefinition PunctuatorDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlKeywordFormat.Name)]
        public static ClassificationTypeDefinition KeywordDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlOperationNameFormat.Name)]
        public static ClassificationTypeDefinition OperationNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlFragmentNameFormat.Name)]
        public static ClassificationTypeDefinition FragmentNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlVariableNameFormat.Name)]
        public static ClassificationTypeDefinition VariableNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlDirectiveNameFormat.Name)]
        public static ClassificationTypeDefinition DirectiveNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlTypeNameFormat.Name)]
        public static ClassificationTypeDefinition TypeNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlFieldNameFormat.Name)]
        public static ClassificationTypeDefinition FieldNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlAliasedFieldNameFormat.Name)]
        public static ClassificationTypeDefinition AliasedFieldNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlArgumentNameFormat.Name)]
        public static ClassificationTypeDefinition ArgumentNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlObjectFieldNameFormat.Name)]
        public static ClassificationTypeDefinition ObjectFieldNameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlStringFormat.Name)]
        public static ClassificationTypeDefinition StringDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlNumberFormat.Name)]
        public static ClassificationTypeDefinition NumberDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlEnumFormat.Name)]
        public static ClassificationTypeDefinition EnumDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlNameFormat.Name)]
        public static ClassificationTypeDefinition NameDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlCommentFormat.Name)]
        public static ClassificationTypeDefinition CommentDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(GqlErrorFormat.Name)]
        public static ClassificationTypeDefinition ErrorDefinition;
    }
}
