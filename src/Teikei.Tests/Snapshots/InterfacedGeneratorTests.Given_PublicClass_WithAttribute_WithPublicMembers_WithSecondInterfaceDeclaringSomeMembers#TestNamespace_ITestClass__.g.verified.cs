﻿//HintName: TestNamespace_ITestClass__.g.cs
namespace TestNamespace
{
	public interface ITestClass
	{
		void TestMethod(string p1, int p2);
		int TestMethod2(bool p3);
		int TestProp1 { get; set; }

		string TestProp2 { get; set; }

		string TestProp6 { get; }

		string TestProp7 { set; }

		string TestProp8 { get; }
	}

	public partial class TestClass : ITestClass
	{
	}
}