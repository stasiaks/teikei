using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis;

internal static class RoslynExtensions
{
	public static TypeSyntax GetSyntax(this ITypeSymbol typeSymbol) =>
		SyntaxFactory.ParseTypeName(
			typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
		);

	public static TypeSyntax GetSyntax(this Type type)
	{
		SyntaxKind? predefinedKind = null;
		if (type == typeof(string))
		{
			predefinedKind = SyntaxKind.StringKeyword;
		}
		else if (type == typeof(bool))
		{
			predefinedKind = SyntaxKind.BoolKeyword;
		}

		return predefinedKind is not null
			? SyntaxFactory.PredefinedType(SyntaxFactory.Token(predefinedKind.Value))
			: SyntaxFactory.ParseTypeName(type.FullName);
	}

	public static LiteralExpressionSyntax? GetExplicitDefaultValueSyntax(
		this IParameterSymbol parameterSymbol
	)
	{
		if (!parameterSymbol.HasExplicitDefaultValue)
		{
			return null;
		}

		return parameterSymbol.ExplicitDefaultValue switch
		{
			null
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.DefaultLiteralExpression,
					SyntaxFactory.Token(SyntaxKind.DefaultKeyword)
				), // valid for both class and struct
			true => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
			false => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
			char v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.CharacterLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			decimal v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			double v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			string v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.StringLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			int v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			long v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			float v
				=> SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(v)
				),
			_
				=> throw new NotImplementedException(
					$"Did not properly implement handling default value for {parameterSymbol.Type.Name}"
				)
		};
	}
}
