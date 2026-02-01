using System.Runtime.CompilerServices;

using Teikei;

public static class ModuleInitializer
{
	// Exists just to ensure these cannot be stripped from assembly
	private static Type[] UsedAttributes => [typeof(InterfacedAttribute), typeof(GenerateSutAttribute)];

	[ModuleInitializer]
	public static void Initialize()
	{
		Console.WriteLine("Tests will use following types:");
		foreach (var t in UsedAttributes)
		{
			Console.WriteLine($"    {t.Name}");
		}
		UseProjectRelativeDirectory("Snapshots");
		VerifySourceGenerators.Initialize();
	}
}
