using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;

namespace GraphQLTools.Utils
{
    internal static class RoslynExtensions
    {
        public static bool IsGqlString(this SemanticModel semanticModel, ArgumentSyntax argument)
        {
            IOperation operation = semanticModel.GetOperation(argument);
            if (operation is IArgumentOperation argumentOperation && argumentOperation.Parameter is IParameterSymbol parameter && parameter.Type.SpecialType == SpecialType.System_String)
            {
                ImmutableArray<AttributeData> parameterAttributes = parameter.GetAttributes();
                foreach (AttributeData attribute in parameterAttributes)
                {
                    INamedTypeSymbol attributeClass = attribute.AttributeClass;
                    if (attributeClass is null)
                        continue;

                    if (attributeClass.Name != "StringSyntaxAttribute")
                        continue;

                    INamespaceSymbol @namespace = attributeClass.ContainingNamespace;
                    if (@namespace is null)
                        continue;

                    if (@namespace.Name != "CodeAnalysis")
                        continue;

                    @namespace = @namespace.ContainingNamespace;
                    if (@namespace is null)
                        continue;

                    if (@namespace.Name != "Diagnostics")
                        continue;

                    @namespace = @namespace.ContainingNamespace;
                    if (@namespace is null)
                        continue;

                    if (@namespace.Name != "System")
                        continue;

                    @namespace = @namespace.ContainingNamespace;
                    if (@namespace is null)
                        continue;

                    if (!@namespace.IsGlobalNamespace)
                        continue;

                    TypedConstant constructorArgument = attribute.ConstructorArguments[0];
                    if (constructorArgument.Type is null || constructorArgument.Type.SpecialType != SpecialType.System_String)
                        continue;

                    if (constructorArgument.Value is string value && value.Equals("graphql", StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}
