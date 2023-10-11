using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace GraphQLTools.Classification
{
    [Export]
    internal class GqlClassificationTypes : IDisposable
    {
        private static bool s_isDarkMode;

        static GqlClassificationTypes()
        {
            s_isDarkMode = IsDarkMode();
        }

        public static ref readonly Color GetThemeAwareColor(in Color lightModeColor, in Color darkModeColor) => ref s_isDarkMode ? ref darkModeColor : ref lightModeColor;

        private static bool IsDarkMode() => VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey).GetBrightness() < 0.5;

        private readonly IClassificationFormatMapService _formatMap;
        private readonly IClassificationTypeRegistryService _typeRegistry;

        [ImportingConstructor]
        public GqlClassificationTypes(IClassificationFormatMapService formatMap, IClassificationTypeRegistryService typeRegistry)
        {
            _formatMap = formatMap;
            _typeRegistry = typeRegistry;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public IClassificationType Punctuator       => _typeRegistry.GetClassificationType(GqlPunctuatorFormat.Name);
        public IClassificationType Keyword          => _typeRegistry.GetClassificationType(GqlKeywordFormat.Name);
        public IClassificationType OperationName    => _typeRegistry.GetClassificationType(GqlOperationNameFormat.Name);
        public IClassificationType FragmentName     => _typeRegistry.GetClassificationType(GqlFragmentNameFormat.Name);
        public IClassificationType VariableName     => _typeRegistry.GetClassificationType(GqlVariableNameFormat.Name);
        public IClassificationType DirectiveName    => _typeRegistry.GetClassificationType(GqlDirectiveNameFormat.Name);
        public IClassificationType TypeName         => _typeRegistry.GetClassificationType(GqlTypeNameFormat.Name);
        public IClassificationType FieldName        => _typeRegistry.GetClassificationType(GqlFieldNameFormat.Name);
        public IClassificationType AliasedFieldName => _typeRegistry.GetClassificationType(GqlAliasedFieldNameFormat.Name);
        public IClassificationType ArgumentName     => _typeRegistry.GetClassificationType(GqlArgumentNameFormat.Name);
        public IClassificationType ObjectFieldName  => _typeRegistry.GetClassificationType(GqlObjectFieldNameFormat.Name);
        public IClassificationType String           => _typeRegistry.GetClassificationType(GqlStringFormat.Name);
        public IClassificationType Number           => _typeRegistry.GetClassificationType(GqlNumberFormat.Name);
        public IClassificationType Enum             => _typeRegistry.GetClassificationType(GqlEnumFormat.Name);
        public IClassificationType Name             => _typeRegistry.GetClassificationType(GqlNameFormat.Name);
        public IClassificationType Comment          => _typeRegistry.GetClassificationType(GqlCommentFormat.Name);
        public IClassificationType Error            => _typeRegistry.GetClassificationType(GqlErrorFormat.Name);

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            s_isDarkMode = IsDarkMode();
            IClassificationFormatMap formatMap = _formatMap.GetClassificationFormatMap("text");

            TextFormattingRunProperties properties;
            formatMap.BeginBatchUpdate();
            try
            {
                properties = formatMap.GetTextProperties(Punctuator);
                formatMap.SetTextProperties(Punctuator, properties.SetForegroundBrush(new SolidColorBrush(GqlPunctuatorFormat.Color)));

                properties = formatMap.GetTextProperties(OperationName);
                formatMap.SetTextProperties(OperationName, properties.SetForegroundBrush(new SolidColorBrush(GqlOperationNameFormat.Color)));

                properties = formatMap.GetTextProperties(Keyword);
                formatMap.SetTextProperties(Keyword, properties.SetForegroundBrush(new SolidColorBrush(GqlKeywordFormat.Color)));

                properties = formatMap.GetTextProperties(FragmentName);
                formatMap.SetTextProperties(FragmentName, properties.SetForegroundBrush(new SolidColorBrush(GqlFragmentNameFormat.Color)));

                properties = formatMap.GetTextProperties(VariableName);
                formatMap.SetTextProperties(VariableName, properties.SetForegroundBrush(new SolidColorBrush(GqlVariableNameFormat.Color)));

                properties = formatMap.GetTextProperties(DirectiveName);
                formatMap.SetTextProperties(DirectiveName, properties.SetForegroundBrush(new SolidColorBrush(GqlDirectiveNameFormat.Color)));

                properties = formatMap.GetTextProperties(TypeName);
                formatMap.SetTextProperties(TypeName, properties.SetForegroundBrush(new SolidColorBrush(GqlTypeNameFormat.Color)));

                properties = formatMap.GetTextProperties(FieldName);
                formatMap.SetTextProperties(FieldName, properties.SetForegroundBrush(new SolidColorBrush(GqlFieldNameFormat.Color)));

                properties = formatMap.GetTextProperties(AliasedFieldName);
                formatMap.SetTextProperties(AliasedFieldName, properties.SetForegroundBrush(new SolidColorBrush(GqlAliasedFieldNameFormat.Color)));

                properties = formatMap.GetTextProperties(ArgumentName);
                formatMap.SetTextProperties(ArgumentName, properties.SetForegroundBrush(new SolidColorBrush(GqlArgumentNameFormat.Color)));

                properties = formatMap.GetTextProperties(ObjectFieldName);
                formatMap.SetTextProperties(ObjectFieldName, properties.SetForegroundBrush(new SolidColorBrush(GqlObjectFieldNameFormat.Color)));

                properties = formatMap.GetTextProperties(String);
                formatMap.SetTextProperties(String, properties.SetForegroundBrush(new SolidColorBrush(GqlStringFormat.Color)));

                properties = formatMap.GetTextProperties(Number);
                formatMap.SetTextProperties(Number, properties.SetForegroundBrush(new SolidColorBrush(GqlNumberFormat.Color)));

                properties = formatMap.GetTextProperties(Enum);
                formatMap.SetTextProperties(Enum, properties.SetForegroundBrush(new SolidColorBrush(GqlEnumFormat.Color)));

                properties = formatMap.GetTextProperties(Name);
                formatMap.SetTextProperties(Name, properties.SetForegroundBrush(new SolidColorBrush(GqlNameFormat.Color)));

                properties = formatMap.GetTextProperties(Comment);
                formatMap.SetTextProperties(Comment, properties.SetForegroundBrush(new SolidColorBrush(GqlCommentFormat.Color)));

                properties = formatMap.GetTextProperties(Error);
                formatMap.SetTextProperties(Error, properties.SetForegroundBrush(new SolidColorBrush(GqlErrorFormat.Color)));
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
        }
    }
}
