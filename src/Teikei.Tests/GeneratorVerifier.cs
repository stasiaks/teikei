using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Teikei.Tests;

public static class GeneratorVerifier
{
	public static Task Verify<TGenerator>(string[] sources)
		where TGenerator : IIncrementalGenerator, new()
	{
		var references = new[]
		{
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
		};

		var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s));

		var compilation = CSharpCompilation.Create("TestAssembly", syntaxTrees, references);

		var generator = new TGenerator();

		var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

		return Verifier.Verify(driver);
	}
}
