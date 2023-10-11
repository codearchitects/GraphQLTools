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
            ForegroundColor = ThemeAwareColorCore;
        }

        protected abstract string NameCore { get; }

        protected abstract Color ThemeAwareColorCore { get; }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlPunctuatorFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Punctuator";
        public static readonly Color LightModeColor = Color.FromRgb(20, 20, 20);
        public static readonly Color DarkModeColor = Color.FromRgb(221, 221, 221);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlKeywordFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Keyword";
        public static readonly Color LightModeColor = Color.FromRgb(177, 26, 4);
        public static readonly Color DarkModeColor = Color.FromRgb(74, 162, 225);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlOperationNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Operation name";
        public static readonly Color LightModeColor = Color.FromRgb(210, 5, 78);
        public static readonly Color DarkModeColor = Color.FromRgb(252, 222, 143);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlFragmentNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Fragment name";
        public static readonly Color LightModeColor = Color.FromRgb(31, 209, 214);
        public static readonly Color DarkModeColor = Color.FromRgb(217, 251, 145);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlVariableNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Variable name";
        public static readonly Color LightModeColor = Color.FromRgb(57, 125, 19);
        public static readonly Color DarkModeColor = Color.FromRgb(150, 237, 20);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlDirectiveNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Directive name";
        public static readonly Color LightModeColor = Color.FromRgb(179, 48, 134);
        public static readonly Color DarkModeColor = Color.FromRgb(224, 116, 200);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlTypeNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Type name";
        public static readonly Color LightModeColor = Color.FromRgb(202, 152, 0);
        public static readonly Color DarkModeColor = Color.FromRgb(78, 201, 176);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlFieldNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Field name";
        public static readonly Color LightModeColor = Color.FromRgb(31, 97, 160);
        public static readonly Color DarkModeColor = Color.FromRgb(121, 164, 171);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlAliasedFieldNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Aliased field name";
        public static readonly Color LightModeColor = Color.FromRgb(28, 146, 169);
        public static readonly Color DarkModeColor = Color.FromRgb(96, 170, 137);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlArgumentNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Argument name";
        public static readonly Color LightModeColor = Color.FromRgb(139, 43, 185);
        public static readonly Color DarkModeColor = Color.FromRgb(189, 249, 255);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlObjectFieldNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Object field name";
        public static readonly Color LightModeColor = Color.FromRgb(168, 130, 108);
        public static readonly Color DarkModeColor = Color.FromRgb(218, 197, 216);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlStringFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - String";
        public static readonly Color LightModeColor = Color.FromRgb(214, 66, 146);
        public static readonly Color DarkModeColor = Color.FromRgb(227, 211, 180);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlNumberFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Number";
        public static readonly Color LightModeColor = Color.FromRgb(40, 130, 249);
        public static readonly Color DarkModeColor = Color.FromRgb(181, 206, 168);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlEnumFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Enum";
        public static readonly Color LightModeColor = Color.FromRgb(140, 200, 20);
        public static readonly Color DarkModeColor = Color.FromRgb(190, 183, 255);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlNameFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Name (unclassified)";
        public static readonly Color LightModeColor = Color.FromRgb(20, 20, 20);
        public static readonly Color DarkModeColor = Color.FromRgb(192, 192, 192);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlCommentFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Comment";
        public static readonly Color LightModeColor = Color.FromRgb(157, 157, 157);
        public static readonly Color DarkModeColor = Color.FromRgb(157, 157, 157);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [UserVisible(true)]
    [Order(After = Priority.High, Before = Priority.High)]
    internal sealed class GqlErrorFormat : GqlClassificationFormatDefinition
    {
        public const string Name = "GraphQL - Error";
        public static readonly Color LightModeColor = Color.FromRgb(200, 0, 0);
        public static readonly Color DarkModeColor = Color.FromRgb(200, 0, 0);
        public static Color ThemeAwareColor => GqlClassificationTypes.GetThemeAwareColor(in LightModeColor, in DarkModeColor);

        protected override string NameCore => Name;

        protected override Color ThemeAwareColorCore => ThemeAwareColor;
    }
}
