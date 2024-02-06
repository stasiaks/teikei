﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

[Generator]
public class InterfacedGenerator : IIncrementalGenerator
{
	private const string AttributesNamespace = "Teikei";
	private const string AttributeName = "Interfaced";
	private const string SkipOverlappingMembersParameterName = "SkipOverlappingMembers";
	private const string ForcePublicAccessibilityParameterName = "ForcePublicAccessibility";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var builder = new AttributeDeclarationBuilder(AttributeName)
			.WithTargets(AttributeTargets.Class)
			.WithParameter(SkipOverlappingMembersParameterName, true)
			.WithParameter(ForcePublicAccessibilityParameterName, false);
		var attribute = builder.Build();

		var namespaceNode = SyntaxFactory
			.NamespaceDeclaration(SyntaxFactory.IdentifierName(AttributesNamespace))
			.WithLeadingTrivia(
				SyntaxFactory.SyntaxTrivia(
					SyntaxKind.SingleLineCommentTrivia,
					"// <auto-generated />"
				)
			)
			.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(attribute));

		var source = namespaceNode.NormalizeWhitespace("\t", "\n").ToFullString();

		context.RegisterPostInitializationOutput(pi =>
			pi.AddSource($"{AttributesNamespace}_{AttributeName}__.g.cs", source)
		);

		var types = context
			.SyntaxProvider.CreateSyntaxProvider(IsMatchingAttribute, Transform)
			.Where(x => x is not null)
			.Collect();

		context.RegisterSourceOutput(types, GenerateInterface!);
	}

	private static void GenerateInterface(
		SourceProductionContext context,
		ImmutableArray<ITypeSymbol> symbols
	)
	{
		foreach (var typeSymbol in symbols)
		{
			var attribute = typeSymbol
				.GetAttributes()
				.FirstOrDefault(x => x.AttributeClass?.Name is $"{AttributeName}Attribute");
			if (attribute is null)
				continue;

			var skipOverlappingMembers = attribute.ConstructorArguments[0].Value as bool?;
			var forcePublicAccessibility = attribute.ConstructorArguments[1].Value as bool?;

			var interfaceName = $"I{typeSymbol.Name}";
			var baseAccessorSyntax =
				typeSymbol.DeclaredAccessibility is Accessibility.Public
					? SyntaxKind.PublicKeyword
					: SyntaxKind.InternalKeyword;

			var interfaceAccessorSyntax = forcePublicAccessibility is true
				? SyntaxKind.PublicKeyword
				: baseAccessorSyntax;

			var publicMembers = typeSymbol
				.GetMembers()
				.Where(x => x.DeclaredAccessibility is Accessibility.Public);

			if (skipOverlappingMembers is true)
			{
				static string GetComparableName(ISymbol symbol)
				{
					var fullDisplayString = symbol.ToDisplayString(
						SymbolDisplayFormat.CSharpShortErrorMessageFormat
					);
					var index = fullDisplayString.LastIndexOf(symbol.Name);
					return index >= 0 ? fullDisplayString.Substring(index) : fullDisplayString;
				}

				var baseTypeMembersNames =
					typeSymbol
						.BaseType?.GetMembers()
						.Concat(typeSymbol.AllInterfaces.SelectMany(x => x.GetMembers()))
						.Where(x => x.DeclaredAccessibility is Accessibility.Public)
						.Select(GetComparableName)
						.Distinct()
						.ToList() ?? [];

				publicMembers = publicMembers.Where(member =>
					!baseTypeMembersNames.Exists(baseName => baseName == GetComparableName(member))
				);
			}

			var memberDeclarations = new List<MemberDeclarationSyntax> { };

			memberDeclarations.AddRange(publicMembers.GetOrdinaryMethodDeclarations());
			memberDeclarations.AddRange(publicMembers.GetPropertyDeclarations());

			var namespaceNode = SyntaxFactory
				.NamespaceDeclaration(
					SyntaxFactory.IdentifierName(typeSymbol.ContainingNamespace.ToDisplayString())
				)
				.WithLeadingTrivia(
					SyntaxFactory.SyntaxTrivia(
						SyntaxKind.SingleLineCommentTrivia,
						"// <auto-generated />"
					)
				)
				.WithMembers(
					SyntaxFactory.List<MemberDeclarationSyntax>(
						[
							SyntaxFactory
								.InterfaceDeclaration(interfaceName)
								.WithModifiers(
									SyntaxFactory.TokenList(
										SyntaxFactory.Token(interfaceAccessorSyntax)
									)
								)
								.WithMembers(SyntaxFactory.List(memberDeclarations)),
							SyntaxFactory
								.ClassDeclaration(typeSymbol.Name)
								.WithModifiers(
									SyntaxFactory.TokenList(
										[
											SyntaxFactory.Token(baseAccessorSyntax),
											SyntaxFactory.Token(SyntaxKind.PartialKeyword)
										]
									)
								)
								.WithBaseList(
									SyntaxFactory.BaseList(
										SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
											SyntaxFactory.SimpleBaseType(
												SyntaxFactory.IdentifierName(interfaceName)
											)
										)
									)
								)
						]
					)
				);

			var source = namespaceNode.NormalizeWhitespace("\t", "\n").ToFullString();

			context.AddSource($"{typeSymbol.ContainingNamespace}_{interfaceName}__.g.cs", source);
		}
	}

	private static bool IsMatchingAttribute(SyntaxNode node, CancellationToken token)
	{
		if (node is not AttributeSyntax attribute)
			return false;

		return attribute.Name switch
		{
			SimpleNameSyntax sns => sns.Identifier.Text is AttributeName,
			QualifiedNameSyntax qns => qns.Right.Identifier.Text is AttributeName,
			_ => false
		};
	}

	private static ITypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken token)
	{
		if (context.Node is not AttributeSyntax attribute)
			return null;

		var parentSymbol = attribute.Parent?.Parent;

		if (parentSymbol is not BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
			return null;

		return context.SemanticModel.GetDeclaredSymbol(baseTypeDeclarationSyntax, token);
	}
}
