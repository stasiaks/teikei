using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

internal static partial class SymbolConversions
{
	public static IEnumerable<PropertyDeclarationSyntax> GetPropertyDeclarations(
		this IEnumerable<ISymbol> symbols
	)
	{
		var properties = symbols.OfType<IPropertySymbol>();

		foreach (var property in properties)
		{
			var accessors = new List<AccessorDeclarationSyntax> { };

			if (property.GetMethod?.DeclaredAccessibility == Accessibility.Public)
			{
				accessors.Add(
					SyntaxFactory
						.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				);
			}

			if (property.SetMethod?.DeclaredAccessibility == Accessibility.Public)
			{
				accessors.Add(
					SyntaxFactory
						.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				);
			}

			if (accessors.Count == 0)
				continue;

			var propertyDeclaration = SyntaxFactory
				.PropertyDeclaration(property.Type.GetSyntax(), property.Name)
				.WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));

			yield return propertyDeclaration;
		}
	}
}
