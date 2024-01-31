using Teikei.NewType;

namespace Teikei.Tests;

public class DebugTest
{
	/// <summary>
	/// This test only exists for ease of attaching debugger to Initialize
	/// </summary>
	[Fact]
	public void Initialize_Test()
	{
		try
		{
			var sut = new NewTypeGenerator();
			sut.Initialize(default);
		}
		catch { }
	}
}
