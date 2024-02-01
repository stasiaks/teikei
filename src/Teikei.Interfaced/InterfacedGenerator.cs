using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

[Generator]
public class InterfacedGenerator : IIncrementalGenerator
{
	private const string AttributesNamespace = "Teikei";
	private const string AttributeName = "Interfaced";
	private const string SkipImplementedMembersParameterName = "skipImplementedMembers";
	private const string ForcePublicAccessibilityParameterName = "forcePublicAccessibility";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var builder = new AttributeDeclarationBuilder(AttributeName)
			.WithTargets(AttributeTargets.Class)
			.WithParameter(typeof(bool), SkipImplementedMembersParameterName)
			.WithParameter(typeof(bool), ForcePublicAccessibilityParameterName);
		var attribute = builder.Build();

		var namespaceNode = SyntaxFactory.NamespaceDeclaration(
			SyntaxFactory.IdentifierName(AttributesNamespace))
		.WithMembers(
			SyntaxFactory.SingletonList<MemberDeclarationSyntax>(attribute));

		var source = namespaceNode.NormalizeWhitespace("\t", "\n").ToFullString();

		context.RegisterPostInitializationOutput(pi => pi.AddSource($"{AttributesNamespace}_{AttributeName}__.g.cs", source));

		var types = context.SyntaxProvider
			.CreateSyntaxProvider(IsMatchingAttribute, Transform)
			.Where(x => x is not null)
			.Collect();

		context.RegisterSourceOutput(types, GenerateInterface!);
	}

	private static void GenerateInterface(SourceProductionContext context, ImmutableArray<ITypeSymbol> symbols)
	{
		foreach (var typeSymbol in symbols)
		{
			var interfaceName = $"I{typeSymbol.Name}";
			var baseAccessorSyntax = (typeSymbol.DeclaredAccessibility & Accessibility.Internal) != 0 ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword;

			var namespaceNode = SyntaxFactory.NamespaceDeclaration(
				SyntaxFactory.IdentifierName(typeSymbol.ContainingNamespace.ToDisplayString()))
					.WithMembers(
						SyntaxFactory.List<MemberDeclarationSyntax>([
							SyntaxFactory.InterfaceDeclaration(interfaceName)
								.WithModifiers(
									SyntaxFactory.TokenList(
										SyntaxFactory.Token(baseAccessorSyntax))),
							SyntaxFactory.ClassDeclaration(typeSymbol.Name)
								.WithModifiers(
									SyntaxFactory.TokenList([
										SyntaxFactory.Token(baseAccessorSyntax),
										SyntaxFactory.Token(SyntaxKind.PartialKeyword)
									]))
								.WithBaseList(
									SyntaxFactory.BaseList(
										SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
											SyntaxFactory.SimpleBaseType(
												SyntaxFactory.IdentifierName(interfaceName)
												))))]));
			
			var source = namespaceNode.NormalizeWhitespace("\t", "\n").ToFullString();

			context.AddSource($"{typeSymbol.ContainingNamespace}_{interfaceName}__.g.cs", source);
		}
	}

	private static bool IsMatchingAttribute(SyntaxNode node, CancellationToken token)
	{
		if (node is not AttributeSyntax attribute)
			return false;

		var result = attribute.Name switch
		{
			SimpleNameSyntax sns => sns.Identifier.Text == AttributeName,
			QualifiedNameSyntax qns => qns.Right.Identifier.Text == AttributeName,
			_ => false
		};
		return result;
	}

	private static ITypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken token)
	{
		if (context.Node is not AttributeSyntax attribute)
			return null;

		var parentSymbol = attribute.Parent?.Parent;

		if (parentSymbol is not BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
			return null;

		return context.SemanticModel.GetDeclaredSymbol(
			baseTypeDeclarationSyntax,
			token
		);
	}
}
