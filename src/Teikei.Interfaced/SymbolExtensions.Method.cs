using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

internal static partial class SymbolExtensions
{
	public static IEnumerable<MethodDeclarationSyntax> GetOrdinaryMethodDeclarations(
		this IEnumerable<ISymbol> symbols
	)
	{
		var ordinaryMethods = symbols
			.OfType<IMethodSymbol>()
			.Where(x => x.MethodKind is MethodKind.Ordinary);
		foreach (var method in ordinaryMethods)
		{
			var parametersList = method
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

			var methodDeclaration = SyntaxFactory
				.MethodDeclaration(
					method.ReturnType.GetSyntax(),
					SyntaxFactory.Identifier(method.Name)
				)
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				.WithParameterList(
					SyntaxFactory.ParameterList(
						SyntaxFactory.SeparatedList<ParameterSyntax>(parametersList)
					)
				)
				.WithTriviaFrom(method);

			yield return methodDeclaration;
		}
	}
}
