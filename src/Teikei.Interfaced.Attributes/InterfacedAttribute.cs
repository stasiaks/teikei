namespace Teikei;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class InterfacedAttribute : Attribute {
	public InterfacedAttribute(
	    bool SkipOverlappingMembers = true,
	    bool ForcePublicAccessibility = false
)
    {
    }
}
