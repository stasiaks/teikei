//HintName: TestNamespace_ITestClass__.g.cs
namespace TestNamespace
{
	public interface ITestClass
	{
		void TestMethod(string p1);
		int TestMethod2(bool p2);
		global::System.Threading.Tasks.Task<bool> TestMethod3(global::TestNamespace.TestClass p3, int p4);
		global::System.Threading.Tasks.Task TestMethod4(global::System.Exception p5, string p6);
	}

	public partial class TestClass : ITestClass
	{
	}
}