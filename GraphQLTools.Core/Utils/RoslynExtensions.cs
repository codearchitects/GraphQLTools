using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace GraphQLTools.Utils;

internal static class RoslynExtensions
{
    public static bool IsGqlString(this SemanticModel semanticModel, ArgumentSyntax argument, CancellationToken cancellationToken)
    {
        IOperation? operation = semanticModel.GetOperation(argument, cancellationToken);
        if (operation is not IArgumentOperation { Parameter: { Type.SpecialType: SpecialType.System_String } parameter })
            return false;

        return parameter.IsGqlString();
    }

    public static bool IsGqlString(this SemanticModel semanticModel, VariableDeclaratorSyntax variableDeclarator, CancellationToken cancellationToken)
    {
        ISymbol? declaredSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken);
        if (declaredSymbol is not IFieldSymbol)
            return false;

        return declaredSymbol.IsGqlString();
    }

    public static bool IsGqlString(this SemanticModel semanticModel, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        ISymbol? declaredSymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken);
        if (declaredSymbol is not IPropertySymbol)
            return false;

        return declaredSymbol.IsGqlString();
    }

    private static bool IsGqlString(this ISymbol symbol)
    {
        ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
        foreach (AttributeData attribute in attributes)
        {
            INamedTypeSymbol? attributeClass = attribute.AttributeClass;
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

            ImmutableArray<TypedConstant> constructorArguments = attribute.ConstructorArguments;
            if (constructorArguments.Length == 0)
                continue;

            TypedConstant constructorArgument = constructorArguments[0];
            if (constructorArgument.Type is not { SpecialType: SpecialType.System_String })
                continue;

            if (constructorArgument.Value is string value && value.Equals("graphql", StringComparison.InvariantCultureIgnoreCase))
                return true;
        }

        return false;
    }
}
