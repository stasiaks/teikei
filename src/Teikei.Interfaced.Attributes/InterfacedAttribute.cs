namespace Teikei;

/// <summary>
/// When placed on a partial type, it will generate matching interface for it.
/// </summary>
/// <param name="SkipOverlappingMembers">If <c>true</c>, generated interface will only include members that are not part of base class and/or other interface</param>
/// <param name="ForcePublicAccessibility">If <c>true</c>, generated interface will be <c>public</c> even if attributed type is not</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class InterfacedAttribute(
	bool SkipOverlappingMembers = true,
	bool ForcePublicAccessibility = false
) : Attribute { }
