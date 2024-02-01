using Teikei.Interfaced;
namespace Teikei.Tests;

public class InterfacedGeneratorTests
{
	[Fact]
	public Task GivenEmptySource_GeneratesAttribute()
	{
		var source = @"";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task GivenClassWithoutAttribute_DoesntGenerateInterface()
	{
		var source = @"
		namespace TestNamespace;

		public class TestClass
		{

		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task GivenClassWithAttribute_GeneratesInterface()
	{
		var source = @"
		namespace TestNamespace;

		[Interfaced]
		public class TestClass
		{

		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}
}
