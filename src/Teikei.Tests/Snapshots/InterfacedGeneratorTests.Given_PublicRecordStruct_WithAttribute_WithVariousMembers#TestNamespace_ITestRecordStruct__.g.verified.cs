﻿//HintName: TestNamespace_ITestRecordStruct__.g.cs
// <auto-generated />
namespace TestNamespace
{
	public interface ITestRecordStruct
	{
		string TestMethod();
		int TestProp { get; set; }

		event global::System.EventHandler TestEvent;
		int this[string a] { get; }
	}

	public partial record struct TestRecordStruct : ITestRecordStruct;
}