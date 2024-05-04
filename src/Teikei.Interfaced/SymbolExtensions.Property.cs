using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

internal static partial class SymbolExtensions
{
	public static IEnumerable<PropertyDeclarationSyntax> GetPropertyDeclarations(
		this IEnumerable<ISymbol> symbols
	)
	{
		var properties = symbols.OfType<IPropertySymbol>().Where(x => !x.IsIndexer);

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
				var syntaxKind = property.SetMethod.IsInitOnly
					? SyntaxKind.InitAccessorDeclaration
					: SyntaxKind.SetAccessorDeclaration;
				accessors.Add(
					SyntaxFactory
						.AccessorDeclaration(syntaxKind)
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				);
			}

			if (accessors.Count == 0)
				continue;

			var propertyDeclaration = SyntaxFactory
				.PropertyDeclaration(property.Type.GetSyntax(), property.Name)
				.WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)))
				.WithTriviaFrom(property);

			yield return propertyDeclaration;
		}
	}
}
