using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Teikeis;

namespace Teikei.NewType;

[Generator]
public class NewTypeGenerator : ISourceGenerator
{
	private const string AttributesNamespace = "Teikei";
	private const string AttributeName = "NewTypeAttribute";
	private const string BaseTypeParameterName = "T";

	public void Execute(GeneratorExecutionContext context)
	{
	}

	public void Initialize(GeneratorInitializationContext context)
	{
		var builder = new AttributeDeclarationBuilder(AttributeName)
			.WithTargets(AttributeTargets.Assembly)
			.WithTypeParameter(BaseTypeParameterName)
			.WithAllowMultiple();
		var attribute = builder.Build();

		var namespaceNode = SyntaxFactory.NamespaceDeclaration(
			SyntaxFactory.IdentifierName(AttributesNamespace))
		.WithMembers(
			SyntaxFactory.SingletonList<MemberDeclarationSyntax>(attribute));

		var source = namespaceNode.NormalizeWhitespace("\t", "\n").ToFullString();

		context.RegisterForPostInitialization(pi => pi.AddSource($"{AttributesNamespace}_{AttributeName}__.g.cs", source));
	}
}
