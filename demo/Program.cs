namespace Teikei.Demo;

[Interfaced]
public partial class TestClass
{
	public int Testing() => 2;
}

public class Program()
{
	private static void Test(ITestClass test)
	{
		Console.WriteLine($"{nameof(test.Testing)} is {test.Testing()}");
	}

	public static int Main()
	{
		Test(new TestClass());
		return 0;
	}
}
