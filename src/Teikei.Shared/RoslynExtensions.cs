using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis;

internal static class RoslynExtensions
{
	public static TypeSyntax GetSyntax(this ITypeSymbol typeSymbol) =>
		SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

	public static TypeSyntax GetSyntax(this Type type)
	{
		SyntaxKind? predefinedKind = null;
		if(type == typeof(string))
		{
			predefinedKind = SyntaxKind.StringKeyword;
		}
		else if(type == typeof(bool))
		{
			predefinedKind = SyntaxKind.BoolKeyword;
		}

		return predefinedKind is not null
			? SyntaxFactory.PredefinedType(
				SyntaxFactory.Token(predefinedKind.Value))
			: SyntaxFactory.ParseTypeName(type.FullName);
	}
}
