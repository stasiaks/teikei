using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Teikei;

internal static partial class SymbolExtensions
{
	public static SyntaxKind[] ToSyntaxKinds(this Accessibility accessibility) =>
		accessibility switch
		{
			Accessibility.Private => [SyntaxKind.PrivateKeyword],
			Accessibility.ProtectedAndInternal
				=> [SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword],
			Accessibility.Protected => [SyntaxKind.ProtectedKeyword],
			Accessibility.Internal => [SyntaxKind.InternalKeyword],
			Accessibility.ProtectedOrInternal
				=> [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword],
			Accessibility.Public => [SyntaxKind.PublicKeyword],
			_ => [],
		};
}
