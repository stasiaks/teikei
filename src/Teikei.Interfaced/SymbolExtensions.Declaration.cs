using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

internal static partial class SymbolExtensions
{
	public static TypeDeclarationSyntax CreatePartialDeclaration(this INamedTypeSymbol typeSymbol)
	{
		TypeDeclarationSyntax baseDeclaration = (typeSymbol.IsValueType, typeSymbol.IsRecord) switch
		{
			(true, true)
				=> SyntaxFactory
					.RecordDeclaration(
						SyntaxKind.RecordStructDeclaration,
						SyntaxFactory.Token(SyntaxKind.RecordKeyword),
						SyntaxFactory.Identifier(typeSymbol.Name)
					)
					.WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.StructKeyword))
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
			(true, false) => SyntaxFactory.StructDeclaration(typeSymbol.Name),
			(false, true)
				=> SyntaxFactory
					.RecordDeclaration(
						SyntaxKind.RecordDeclaration,
						SyntaxFactory.Token(SyntaxKind.RecordKeyword),
						SyntaxFactory.Identifier(typeSymbol.Name)
					)
					.WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.ClassKeyword))
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
			(false, false) => SyntaxFactory.ClassDeclaration(typeSymbol.Name)
		};

		if (typeSymbol.GetTypeParameterList() is { } typeParameterList)
		{
			baseDeclaration = baseDeclaration.WithTypeParameterList(typeParameterList);
		}

		SyntaxKind[] baseAccessorSyntaxKinds = typeSymbol.DeclaredAccessibility switch
		{
			Accessibility.Private => [SyntaxKind.PrivateKeyword],
			Accessibility.ProtectedAndInternal
				=> [SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword],
			Accessibility.Protected => [SyntaxKind.ProtectedKeyword],
			Accessibility.Internal => [SyntaxKind.InternalKeyword],
			Accessibility.ProtectedOrInternal
				=> [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword],
			Accessibility.Public => [SyntaxKind.PublicKeyword],
			_ => [],
		};

		var tokens = baseAccessorSyntaxKinds
			.Concat([SyntaxKind.PartialKeyword])
			.Select(SyntaxFactory.Token);

		return baseDeclaration.WithModifiers(SyntaxFactory.TokenList(tokens));
	}

	public static TypeParameterListSyntax? GetTypeParameterList(this INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol.Arity <= 0)
			return null;
		var list = typeSymbol
			.TypeParameters.SelectMany<ITypeParameterSymbol, SyntaxNodeOrToken>(x =>
				[SyntaxFactory.Token(SyntaxKind.CommaToken), SyntaxFactory.TypeParameter(x.Name)]
			)
			.Skip(1)
			.ToArray();
		var typeParameterList = SyntaxFactory.TypeParameterList(
			SyntaxFactory.SeparatedList<TypeParameterSyntax>(list)
		);

		return typeParameterList;
	}

	public static TypeArgumentListSyntax? ToTypeArguments(
		this TypeParameterListSyntax parameterListSyntax
	)
	{
		var nodes = parameterListSyntax
			.DescendantNodes()
			.OfType<TypeParameterSyntax>()
			.SelectMany<TypeParameterSyntax, SyntaxNodeOrToken>(x =>

				[
					SyntaxFactory.Token(SyntaxKind.CommaToken),
					SyntaxFactory.IdentifierName(x.Identifier.Text)
				]
			)
			.Skip(1)
			.ToArray();
		return SyntaxFactory.TypeArgumentList(
			SyntaxFactory.SeparatedList<TypeSyntax>(nodes)
		);
	}
}
