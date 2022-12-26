public class AdminDescriptorAttribute : AdminAttribute
{
	public string Descriptor { get; }

	protected AdminDescriptorAttribute(string _Path, string _Descriptor) : base(_Path)
	{
		Descriptor = _Descriptor;
	}
}