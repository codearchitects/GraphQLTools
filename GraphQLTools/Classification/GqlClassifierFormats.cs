using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace GraphQLTools.Classification
{
    internal abstract class GqlClassificationFormatDefinition : ClassificationFormatDefinition
    {
        public GqlClassificationFormatDefinition()
        {
            DisplayName = NameCore;
            ForegroundColor = ColorCore;
        }

        protected abstract string NameCore { get; }

        protected abstract Color ColorCore { get; }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlPunctuatorFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Punctuator";

        private static readonly Color s_lightModeColor = Color.FromRgb(20, 20, 20);
        private static readonly Color s_darkModeColor = Color.FromRgb(221, 221, 221);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlKeywordFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Keyword";

        private static readonly Color s_lightModeColor = Color.FromRgb(177, 26, 4);
        private static readonly Color s_darkModeColor = Color.FromRgb(74, 162, 225);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlOperationNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Operation name";

        private static readonly Color s_lightModeColor = Color.FromRgb(210, 5, 78);
        private static readonly Color s_darkModeColor = Color.FromRgb(252, 222, 143);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlFragmentNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Fragment name";

        private static readonly Color s_lightModeColor = Color.FromRgb(31, 209, 214);
        private static readonly Color s_darkModeColor = Color.FromRgb(217, 251, 145);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlVariableNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Variable name";

        private static readonly Color s_lightModeColor = Color.FromRgb(57, 125, 19);
        private static readonly Color s_darkModeColor = Color.FromRgb(123, 219, 36);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlDirectiveNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Directive name";

        private static readonly Color s_lightModeColor = Color.FromRgb(179, 48, 134);
        private static readonly Color s_darkModeColor = Color.FromRgb(224, 98, 212);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlTypeNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Type name";

        private static readonly Color s_lightModeColor = Color.FromRgb(202, 152, 0);
        private static readonly Color s_darkModeColor = Color.FromRgb(80, 240, 190);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlFieldNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Field name";

        private static readonly Color s_lightModeColor = Color.FromRgb(31, 97, 160);
        private static readonly Color s_darkModeColor = Color.FromRgb(121, 164, 171);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlAliasedFieldNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Aliased field name";

        private static readonly Color s_lightModeColor = Color.FromRgb(28, 146, 169);
        private static readonly Color s_darkModeColor = Color.FromRgb(96, 170, 137);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlArgumentNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Argument name";

        private static readonly Color s_lightModeColor = Color.FromRgb(139, 43, 185);
        private static readonly Color s_darkModeColor = Color.FromRgb(195, 255, 255);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlObjectFieldNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Object field name";

        private static readonly Color s_lightModeColor = Color.FromRgb(168, 130, 108);
        private static readonly Color s_darkModeColor = Color.FromRgb(218, 197, 216);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlStringFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - String";

        private static readonly Color s_lightModeColor = Color.FromRgb(214, 66, 146);
        private static readonly Color s_darkModeColor = Color.FromRgb(227, 211, 180);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlNumberFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Number";

        private static readonly Color s_lightModeColor = Color.FromRgb(40, 130, 249);
        private static readonly Color s_darkModeColor = Color.FromRgb(181, 206, 168);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlEnumFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Enum";

        private static readonly Color s_lightModeColor = Color.FromRgb(140, 200, 20);
        private static readonly Color s_darkModeColor = Color.FromRgb(190, 183, 255);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Name (unclassified)";

        private static readonly Color s_lightModeColor = Color.FromRgb(20, 20, 20);
        private static readonly Color s_darkModeColor = Color.FromRgb(192, 192, 192);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlCommentFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Comment";

        private static readonly Color s_lightModeColor = Color.FromRgb(157, 157, 157);
        private static readonly Color s_darkModeColor = Color.FromRgb(157, 157, 157);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlErrorFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Error";

        private static readonly Color s_lightModeColor = Color.FromRgb(200, 0, 0);
        private static readonly Color s_darkModeColor = Color.FromRgb(230, 0, 0);
        public static ref readonly Color Color => ref GqlClassificationTypes.GetThemeAwareColor(in s_lightModeColor, in s_darkModeColor);

        protected override string NameCore => Name;

        protected override Color ColorCore => Color;
    }
}
