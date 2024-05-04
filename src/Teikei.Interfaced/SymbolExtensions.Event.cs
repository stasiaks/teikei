using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei.Interfaced;

internal static partial class SymbolExtensions
{
	public static IEnumerable<EventFieldDeclarationSyntax> GetEventDeclarations(
		this IEnumerable<ISymbol> symbols
	)
	{
		var events = symbols.OfType<IEventSymbol>();

		foreach (var @event in events)
		{
			var eventDeclaration = SyntaxFactory
				.EventFieldDeclaration(
					SyntaxFactory
						.VariableDeclaration(
							SyntaxFactory.IdentifierName(
								@event.Type.ToDisplayString(
									SymbolDisplayFormat.FullyQualifiedFormat
								)
							)
						)
						.WithVariables(
							SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
								SyntaxFactory.VariableDeclarator(
									SyntaxFactory.Identifier(@event.Name)
								)
							)
						)
				)
				.WithTriviaFrom(@event);

			yield return eventDeclaration;
		}
	}
}
