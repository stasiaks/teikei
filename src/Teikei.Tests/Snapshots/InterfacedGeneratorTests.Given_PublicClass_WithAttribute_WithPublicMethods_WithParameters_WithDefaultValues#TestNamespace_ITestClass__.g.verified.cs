﻿//HintName: TestNamespace_ITestClass__.g.cs
namespace TestNamespace
{
	public interface ITestClass
	{
		void TestMethod(string p1 = "testing testing");
		int TestMethod2(bool p2 = true);
		global::System.Threading.Tasks.Task<bool> TestMethod3(global::TestNamespace.TestClass p3 = default, int p4 = 5);
		global::System.Threading.Tasks.Task TestMethod4(global::System.Exception p5, string p6 = default);
	}

	public partial class TestClass : ITestClass
	{
	}
}