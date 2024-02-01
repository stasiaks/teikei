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
	public Task Given_Class_WithoutAttribute_DoesntGenerateInterface()
	{
		var source = @"
		namespace TestNamespace;

		public partial class TestClass
		{

		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_PublicClass_WithAttribute_GeneratesPublicInterface()
	{
		var source = @"
		using Teikei;

		namespace TestNamespace;

		[Interfaced]
		public partial class TestClass
		{

		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_InternalClass_WithAttribute_GeneratesInternalInterface()
	{
		var source = @"
		using Teikei;

		namespace TestNamespace;

		[Interfaced]
		internal partial class TestClass
		{

		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_InternalClass_WithAttribute_WithForcePublic_GeneratesPublicInterface()
	{
		var source = @"
		using Teikei;

		namespace TestNamespace;

		[Interfaced(ForcePublicAccessibility: true)]
		internal partial class TestClass
		{

		}
		";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}
}
