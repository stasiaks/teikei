﻿//HintName: TestNamespace_ITestClass__.g.cs
namespace TestNamespace
{
	public interface ITestClass
	{
		int TestProp1 { get; set; }

		string TestProp2 { get; set; }

		global::System.Threading.Tasks.Task TestProp3 { get; set; }

		bool TestProp4 { get; }

		bool TestProp5 { set; }

		string TestProp6 { get; }

		string TestProp7 { set; }

		string TestProp8 { get; }
	}

	public partial class TestClass : ITestClass
	{
	}
}