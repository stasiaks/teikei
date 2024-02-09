using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

internal static partial class SymbolExtensions
{
	public static IEnumerable<IndexerDeclarationSyntax> GetIndexerDeclarations(
		this IEnumerable<ISymbol> symbols
	)
	{
		var indexers = symbols.OfType<IPropertySymbol>().Where(x => x.IsIndexer);

		foreach (var indexer in indexers)
		{
			var accessors = new List<AccessorDeclarationSyntax> { };

			if (indexer.GetMethod?.DeclaredAccessibility == Accessibility.Public)
			{
				accessors.Add(
					SyntaxFactory
						.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				);
			}

			if (indexer.SetMethod?.DeclaredAccessibility == Accessibility.Public)
			{
				var syntaxKind = indexer.SetMethod.IsInitOnly ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration;
				accessors.Add(
					SyntaxFactory
						.AccessorDeclaration(syntaxKind)
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				);
			}

			if (accessors.Count == 0)
				continue;

			var parametersList = indexer
				.Parameters.SelectMany<IParameterSymbol, SyntaxNodeOrToken>(x =>
				{
					var parameterSyntax = SyntaxFactory
						.Parameter(SyntaxFactory.Identifier(x.Name))
						.WithType(x.Type.GetSyntax());
					var defaultValueSyntax = x.GetExplicitDefaultValueSyntax();
					if (defaultValueSyntax is not null)
					{
						parameterSyntax = parameterSyntax.WithDefault(
							SyntaxFactory.EqualsValueClause(defaultValueSyntax)
						);
					}
					return [SyntaxFactory.Token(SyntaxKind.CommaToken), parameterSyntax];
				})
				.Skip(1)
				.ToArray();

			var indexerDeclaration = SyntaxFactory
				.IndexerDeclaration(indexer.Type.GetSyntax())
				.WithParameterList(
					SyntaxFactory.BracketedParameterList(
						SyntaxFactory.SeparatedList<ParameterSyntax>(parametersList)
					)
				)
				.WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));

			yield return indexerDeclaration;
		}
	}
}
