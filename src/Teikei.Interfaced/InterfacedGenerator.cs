using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

[Generator]
public class InterfacedGenerator : IIncrementalGenerator
{
	private const string AttributesNamespace = "Teikei";
	private const string AttributeName = "Interfaced";
	private const string SkipImplementedMembersParameterName = "SkipImplementedMembers";
	private const string ForcePublicAccessibilityParameterName = "ForcePublicAccessibility";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var builder = new AttributeDeclarationBuilder(AttributeName)
			.WithTargets(AttributeTargets.Class)
			.WithParameter(SkipImplementedMembersParameterName, true)
			.WithParameter(ForcePublicAccessibilityParameterName, false);
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
			var attribute = typeSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name is $"{AttributeName}Attribute");
			if (attribute is null)
				continue;

			var skipImplementedMembers = attribute.ConstructorArguments[0].Value as bool?;
			var forcePublicAccessibility = attribute.ConstructorArguments[1].Value as bool?;

			var interfaceName = $"I{typeSymbol.Name}";
			var baseAccessorSyntax = typeSymbol.DeclaredAccessibility is Accessibility.Public ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword;

			var interfaceAccessorSyntax = forcePublicAccessibility is true ? SyntaxKind.PublicKeyword : baseAccessorSyntax;

			var publicMembers = typeSymbol.GetMembers()
				.Where(x => x.DeclaredAccessibility is Accessibility.Public);

			var methods = publicMembers
				.OfType<IMethodSymbol>()
				.Where(x => x.MethodKind is not MethodKind.PropertySet)
				.Where(x => x.MethodKind is not MethodKind.PropertyGet)
				.Where(x => x.MethodKind is not MethodKind.Constructor);

			var properties = publicMembers
				.OfType<IPropertySymbol>();

			var memberDeclarations = new List<MemberDeclarationSyntax> { };

			foreach (var method in methods)
			{
				var specialType = method.ReturnType.SpecialType;

				var parametersList = method.Parameters
					.SelectMany<IParameterSymbol, SyntaxNodeOrToken>(x =>
					{
						var parameterSyntax = SyntaxFactory.Parameter(
							SyntaxFactory.Identifier(x.Name))
							.WithType(x.Type.GetSyntax());
						var defaultValueSyntax = x.GetExplicitDefaultValueSyntax();
						if (defaultValueSyntax is not null)
						{
							parameterSyntax = parameterSyntax.WithDefault(
								SyntaxFactory.EqualsValueClause(
									defaultValueSyntax
								)
							);
						}
						return [SyntaxFactory.Token(SyntaxKind.CommaToken), parameterSyntax];
					})
					.Skip(1)
					.ToArray();

				var methodDeclaration = SyntaxFactory.MethodDeclaration(
					method.ReturnType.GetSyntax(),
					SyntaxFactory.Identifier(method.Name))
				.WithSemicolonToken(
					SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				.WithParameterList(
					SyntaxFactory.ParameterList(
						SyntaxFactory.SeparatedList<ParameterSyntax>(parametersList))
				);

				memberDeclarations.Add(methodDeclaration);
			}

			var namespaceNode = SyntaxFactory.NamespaceDeclaration(
				SyntaxFactory.IdentifierName(typeSymbol.ContainingNamespace.ToDisplayString()))
					.WithMembers(
						SyntaxFactory.List<MemberDeclarationSyntax>([
							SyntaxFactory.InterfaceDeclaration(interfaceName)
								.WithModifiers(
									SyntaxFactory.TokenList(
										SyntaxFactory.Token(interfaceAccessorSyntax)))
								.WithMembers(
									SyntaxFactory.List(memberDeclarations)),
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

		return context.SemanticModel.GetDeclaredSymbol(
			baseTypeDeclarationSyntax,
			token
		);
	}
}
