using Teikei.GenerateSystemUnderTest;

namespace Teikei.Tests;

public class SutGeneratorTests
{
	#region CommonSources
	private const string ConstructorlessSut =
		@"
		namespace TestNamespace;

		public class SutClass {}
	";
	#endregion

	[Fact]
	public Task GivenEmptySource()
	{
		var source = @"";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_Method_WithAttribute()
	{
		var source =
			@"
		using Teikei;

		namespace TestNamespace;

		public partial class TestClass
		{
			[GenerateSut]
			private partial SutClass GenerateSut();
		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([ConstructorlessSut, source]);
	}
}
