using Microsoft.CodeAnalysis;

namespace Teikei.Interfaced;

internal static partial class SymbolExtensions
{
	public static T WithTriviaFrom<T>(this T syntax, IEventSymbol symbol)
		where T : SyntaxNode
	{
		var syntaxNodeWithStructuredTrivia = symbol
			.DeclaringSyntaxReferences.Select(x => x.GetSyntax().Parent?.Parent)
			.FirstOrDefault(x => x?.HasStructuredTrivia ?? false);

		return syntaxNodeWithStructuredTrivia is not null
			? syntax.WithTriviaFrom(syntaxNodeWithStructuredTrivia)
			: syntax;
	}

	public static T WithTriviaFrom<T>(this T syntax, ISymbol symbol)
		where T : SyntaxNode
	{
		var syntaxNodeWithStructuredTrivia = symbol
			.DeclaringSyntaxReferences.Select(x => x.GetSyntax())
			.FirstOrDefault(x => x.HasStructuredTrivia);

		return syntaxNodeWithStructuredTrivia is not null
			? syntax.WithTriviaFrom(syntaxNodeWithStructuredTrivia)
			: syntax;
	}
}
