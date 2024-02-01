using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei;

internal class AttributeDeclarationBuilder(string name)
{
	private readonly List<KeyValuePair<Type, string>> parametersList = [];
	private readonly List<string> _typeParameters = [];

	private AttributeTargets _targets = AttributeTargets.All;

	private bool allowMultiple = false;

	public AttributeDeclarationBuilder WithTargets(AttributeTargets newTargets)
	{
		_targets = newTargets;
		return this;
	}

	public AttributeDeclarationBuilder WithParameter(Type parameterType, string parameterName)
	{
		parametersList.Add(new KeyValuePair<Type, string>(parameterType, parameterName));
		return this;
	}

	public AttributeDeclarationBuilder WithAllowMultiple(bool allow = true)
	{
		allowMultiple = allow;
		return this;
	}

	public AttributeDeclarationBuilder WithTypeParameter(string parameterName)
	{
		_typeParameters.Add(parameterName);
		return this;
	}

	public ClassDeclarationSyntax Build()
	{
		var declaration = SyntaxFactory.ClassDeclaration($"{name}Attribute")
			.WithModifiers(
				SyntaxFactory.TokenList(
					SyntaxFactory.Token(SyntaxKind.InternalKeyword)))
			.WithBaseList(
				SyntaxFactory.BaseList(
					SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
						SyntaxFactory.SimpleBaseType(
							SyntaxFactory.QualifiedName(
								SyntaxFactory.IdentifierName(nameof(System)),
								SyntaxFactory.IdentifierName(nameof(Attribute)))))))
			.WithAttributeLists(
				SyntaxFactory.SingletonList<AttributeListSyntax>(
					SyntaxFactory.AttributeList(
						SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
							SyntaxFactory.Attribute(
								SyntaxFactory.QualifiedName(
									SyntaxFactory.IdentifierName(nameof(System)),
									SyntaxFactory.IdentifierName("AttributeUsage")
								)
							)
							.WithArgumentList(
								SyntaxFactory.AttributeArgumentList(
									SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
										new SyntaxNodeOrToken[]{
											SyntaxFactory.AttributeArgument(
												CreateAttributeTargetsExpression()
											),
											SyntaxFactory.Token(SyntaxKind.CommaToken),
											SyntaxFactory.AttributeArgument(
												SyntaxFactory.LiteralExpression(allowMultiple
													? SyntaxKind.TrueLiteralExpression
													: SyntaxKind.FalseLiteralExpression)
											)
											.WithNameEquals(
												SyntaxFactory.NameEquals(
													SyntaxFactory.IdentifierName(nameof(AttributeUsageAttribute.AllowMultiple))
												)
											),
										}
									)
								)
							)
						)
					)
			));
		if (_typeParameters.Count > 0)
		{
			var typeParameterNodes = _typeParameters
				.SelectMany(x => new SyntaxNodeOrToken[]{
					SyntaxFactory.Token(SyntaxKind.CommaToken),
					SyntaxFactory.TypeParameter(
						SyntaxFactory.Identifier(x)
					)})
				.Skip(1)
				.ToArray();

			declaration = declaration.WithTypeParameterList(
				SyntaxFactory.TypeParameterList(
					SyntaxFactory.SeparatedList<TypeParameterSyntax>(typeParameterNodes)
				)
			);
		}


		return declaration;
	}

	private ExpressionSyntax CreateAttributeTargetsExpression()
	{
		var values = _targets.ToString("f")
			.Split(',')
			.Select(x => x.Trim())
			.Select(x => SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.IdentifierName(nameof(System)),
					SyntaxFactory.IdentifierName(nameof(AttributeTargets))
				),
				SyntaxFactory.IdentifierName(x)))
			.ToArray();

		if (values.Length == 1)
		{
			return values[0];
		}

		var firstBitwise = SyntaxFactory.BinaryExpression(
			SyntaxKind.BitwiseOrExpression,
			values[0],
			values[1]
		);
		var remaining = values.Skip(2).ToArray();

		return remaining.Aggregate(firstBitwise, (acc, next) =>
			SyntaxFactory.BinaryExpression(
				SyntaxKind.BitwiseOrExpression,
				acc,
				next
			));
	}
}
