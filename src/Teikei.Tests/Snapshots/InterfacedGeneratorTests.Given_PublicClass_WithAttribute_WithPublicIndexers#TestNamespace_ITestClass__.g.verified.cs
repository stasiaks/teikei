﻿//HintName: TestNamespace_ITestClass__.g.cs
// <auto-generated />
namespace TestNamespace
{
	public interface ITestClass
	{
		int this[bool a] { get; }

		int this[int a] { get; set; }

		string this[string a] { set; }

		bool this[string a, int b] { set; }
	}

	public partial class TestClass : ITestClass
	{
	}
}