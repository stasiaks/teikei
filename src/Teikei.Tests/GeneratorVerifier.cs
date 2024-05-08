using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Teikei.Tests;

[Interfaced(SkipOverlappingMembers: true)]
public static class GeneratorVerifier
{
	public static Task Verify<TGenerator>(string[] sources)
		where TGenerator : IIncrementalGenerator, new()
	{
		var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s));

		var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, References);

		var generator = new TGenerator();

		var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

		var result = driver.GetRunResult();

		return Verifier.Verify(result);
	}

	public static readonly List<PortableExecutableReference> References = AppDomain
		.CurrentDomain.GetAssemblies()
		.Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
		.Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
		.ToList();
}
