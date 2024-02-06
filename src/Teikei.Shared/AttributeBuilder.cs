using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei;

internal class AttributeDeclarationBuilder(string name)
{
	private readonly List<KeyValuePair<Type, string>> _parametersList = [];
	private readonly Dictionary<string, object?> _parameterDefaultValues = [];
	private readonly List<string> _typeParameters = [];

	private AttributeTargets _targets = AttributeTargets.All;

	private bool _allowMultiple = false;

	public AttributeDeclarationBuilder WithTargets(AttributeTargets newTargets)
	{
		_targets = newTargets;
		return this;
	}

	public AttributeDeclarationBuilder WithParameter<T>(string parameterName)
	{
		if (_parameterDefaultValues.Count != 0)
			throw new InvalidOperationException(
				"Can't add parameter without default value after one with default value"
			);
		_parametersList.Add(new KeyValuePair<Type, string>(typeof(T), parameterName));
		return this;
	}

	public AttributeDeclarationBuilder WithParameter<T>(string parameterName, T defaultValue)
	{
		_parametersList.Add(new KeyValuePair<Type, string>(typeof(T), parameterName));
		_parameterDefaultValues.Add(parameterName, defaultValue);
		return this;
	}

	public AttributeDeclarationBuilder WithAllowMultiple(bool allow = true)
	{
		_allowMultiple = allow;
		return this;
	}

	public AttributeDeclarationBuilder WithTypeParameter(string parameterName)
	{
		_typeParameters.Add(parameterName);
		return this;
	}

	public ClassDeclarationSyntax Build()
	{
		var className = $"{name}Attribute";
		var declaration = SyntaxFactory
			.ClassDeclaration(className)
			.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword)))
			.WithBaseList(
				SyntaxFactory.BaseList(
					SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
						SyntaxFactory.SimpleBaseType(
							SyntaxFactory.QualifiedName(
								SyntaxFactory.IdentifierName(nameof(System)),
								SyntaxFactory.IdentifierName(nameof(Attribute))
							)
						)
					)
				)
			)
			.WithAttributeLists(
				SyntaxFactory.SingletonList<AttributeListSyntax>(
					SyntaxFactory.AttributeList(
						SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
							SyntaxFactory
								.Attribute(
									SyntaxFactory.QualifiedName(
										SyntaxFactory.IdentifierName(nameof(System)),
										SyntaxFactory.IdentifierName("AttributeUsage")
									)
								)
								.WithArgumentList(
									SyntaxFactory.AttributeArgumentList(
										SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
											new SyntaxNodeOrToken[]
											{
												SyntaxFactory.AttributeArgument(
													CreateAttributeTargetsExpression()
												),
												SyntaxFactory.Token(SyntaxKind.CommaToken),
												SyntaxFactory
													.AttributeArgument(
														SyntaxFactory.LiteralExpression(
															_allowMultiple
																? SyntaxKind.TrueLiteralExpression
																: SyntaxKind.FalseLiteralExpression
														)
													)
													.WithNameEquals(
														SyntaxFactory.NameEquals(
															SyntaxFactory.IdentifierName(
																nameof(
																	AttributeUsageAttribute.AllowMultiple
																)
															)
														)
													),
											}
										)
									)
								)
						)
					)
				)
			);
		if (_typeParameters.Count > 0)
		{
			var typeParameterNodes = _typeParameters
				.SelectMany(x =>
					new SyntaxNodeOrToken[]
					{
						SyntaxFactory.Token(SyntaxKind.CommaToken),
						SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(x))
					}
				)
				.Skip(1)
				.ToArray();

			declaration = declaration.WithTypeParameterList(
				SyntaxFactory.TypeParameterList(
					SyntaxFactory.SeparatedList<TypeParameterSyntax>(typeParameterNodes)
				)
			);
		}
		if (_parametersList.Count > 0)
		{
			var parameterNodes = _parametersList
				.SelectMany(x =>
				{
					var hasDefaultValue = _parameterDefaultValues.TryGetValue(
						x.Value,
						out var defaultValue
					);
					var parameterSyntax = SyntaxFactory
						.Parameter(SyntaxFactory.Identifier(x.Value))
						.WithType(x.Key.GetSyntax());

					if (hasDefaultValue)
					{
						parameterSyntax = parameterSyntax.WithDefault(
							SyntaxFactory.EqualsValueClause(CreateLiteralExpression(defaultValue))
						);
					}

					return new SyntaxNodeOrToken[]
					{
						SyntaxFactory.Token(SyntaxKind.CommaToken),
						parameterSyntax
					};
				})
				.Skip(1)
				.ToArray();

			declaration = declaration.WithMembers(
				SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
					SyntaxFactory
						.ConstructorDeclaration(SyntaxFactory.Identifier(className))
						.WithParameterList(
							SyntaxFactory.ParameterList(
								SyntaxFactory.SeparatedList<ParameterSyntax>(parameterNodes)
							)
						)
						.WithModifiers(
							SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
						)
						.WithBody(SyntaxFactory.Block()) // No need for any body
				)
			);
		}

		return declaration;
	}

	private ExpressionSyntax CreateAttributeTargetsExpression()
	{
		var values = _targets
			.ToString("f")
			.Split(',')
			.Select(x => x.Trim())
			.Select(x =>
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.IdentifierName(nameof(System)),
						SyntaxFactory.IdentifierName(nameof(AttributeTargets))
					),
					SyntaxFactory.IdentifierName(x)
				)
			)
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

		return remaining.Aggregate(
			firstBitwise,
			(acc, next) => SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, acc, next)
		);
	}

	private LiteralExpressionSyntax CreateLiteralExpression(object? value)
	{
		return value switch
		{
			null => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
			true => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
			false => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
			_
				=> throw new NotImplementedException(
					$"Didn't implement handling of literals with type {value.GetType().Name}"
				)
		};
	}
}
