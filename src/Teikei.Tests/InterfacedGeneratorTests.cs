using Teikei.Interfaced;
namespace Teikei.Tests;

public class InterfacedGeneratorTests
{
	[Fact]
	public Task GivenEmptySource()
	{
		var source = @"";

		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_Class_WithoutAttribute()
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
	public Task Given_PublicClass_WithAttribute()
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
	public Task Given_InternalClass_WithAttribute()
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
	public Task Given_InternalClass_WithAttribute_WithForcePublic()
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

	[Fact]
	public Task Given_PublicClass_WithAttribute_WithPublicMethods()
	{
		var source = @"
		using Teikei;
		using System.Threading.Tasks;

		namespace TestNamespace;

		[Interfaced]
		public partial class TestClass
		{
			public void TestMethod()
			{
				System.Console.WriteLine(""test"");
			}

			public int TestMethod2() => 2;

			public System.Threading.Tasks.Task<bool> TestMethod3() => Task.FromResult(true);

			public async Task TestMethod4()
			{
				return;
			}
		}
		";
		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_PublicClass_WithAttribute_WithPublicMethods_WithParameters()
	{
		var source = @"
		using Teikei;
		using System.Threading.Tasks;

		namespace TestNamespace;

		[Interfaced]
		public partial class TestClass
		{
			public void TestMethod(string p1)
			{
				System.Console.WriteLine(p1);
			}

			public int TestMethod2(bool p2) => p2 ? 6 : -1;

			public System.Threading.Tasks.Task<bool> TestMethod3(TestClass p3, int p4) => Task.FromResult(p4 == 1);

			public async Task TestMethod4(System.Exception p5, string p6)
			{
				throw p5;
			}
		}
		";
		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_PublicClass_WithAttribute_WithPublicMethods_WithParameters_WithDefaultValues()
	{
		var source = @"
		using Teikei;
		using System.Threading.Tasks;

		namespace TestNamespace;

		[Interfaced]
		public partial class TestClass
		{
			public void TestMethod(string p1 = ""testing testing"")
			{
				System.Console.WriteLine(p1);
			}

			public int TestMethod2(bool p2 = true) => p2 ? 6 : -1;

			public System.Threading.Tasks.Task<bool> TestMethod3(TestClass? p3 = null, int p4 = 5) => Task.FromResult(p4 == 1);

			public async Task TestMethod4(System.Exception? p5, string p6 = default)
			{
				throw p5;
			}
		}
		";
		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}

	[Fact]
	public Task Given_PublicClass_WithAttribute_WithPublicProperties()
	{
		var source = @"
		using Teikei;
		using System.Threading.Tasks;

		namespace TestNamespace;

		[Interfaced]
		public partial class TestClass
		{
			public int TestProp1 { get; set; }
			public string TestProp2 { get; set; }
			public Task TestProp3 { get; set; }
			public bool TestProp4 { get; }
			public bool TestProp5 { set; }
			public string TestProp6 { get; private set; }
			public string TestProp7 { protected get; set; }
			public string TestProp8 => ""test"";
		}
		";
		return GeneratorVerifier.Verify<InterfacedGenerator>([source]);
	}
}
