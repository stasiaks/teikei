﻿//HintName: TestNamespace_ITestClass__.g.cs
namespace TestNamespace
{
	public interface ITestClass
	{
		void TestMethod();
		int TestMethod2();
		global::System.Threading.Tasks.Task<bool> TestMethod3();
		global::System.Threading.Tasks.Task TestMethod4();
	}

	public partial class TestClass : ITestClass
	{
	}
}