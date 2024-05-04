using Demo;

namespace Teikei.Demo;

[Interfaced(SkipOverlappingMembers: false, ForcePublicAccessibility: true)]
public partial class TestClass
{
	public int Testing() => 2;

	public event EventHandler<int> testEvent;
}

public class Program()
{
	private static void Test(ITestClass test)
	{
		Console.WriteLine($"{nameof(test.Testing)} is {test.Testing()}");
	}

	private static void Test(IPublicService service)
	{
		Console.WriteLine(
			$"{nameof(service.FullyPublicProperty)} is {service.FullyPublicProperty}"
		);
	}

	public static int Main()
	{
		Test(new TestClass());
		Test(new PublicService() { FullyPublicProperty = 3 });
		return 0;
	}
}
