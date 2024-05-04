using Teikei;

namespace Demo;

[Interfaced]
public partial record PublicService
{
	private Exception FullyPrivateProperty { get; set; }
	
	public Exception PrivateGetSetProperty { private get; init; }

	public int FullyPublicProperty { get; set; }

	public string PublicGetterPrivateSetter { get; private set; }

	public IEnumerable<string> PrivateGetterPublicSetter { private get; set; }

}
