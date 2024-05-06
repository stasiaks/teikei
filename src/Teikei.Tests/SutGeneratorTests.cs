using Teikei.GenerateSystemUnderTest;

namespace Teikei.Tests;

public class SutGeneratorTests
{
	[Fact]
	public Task GivenEmptySource()
	{
		var source = @"";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}
}
