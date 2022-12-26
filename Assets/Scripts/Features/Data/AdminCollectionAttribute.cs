public class AdminCollectionAttribute : AdminAttribute
{
	public string Mask { get; }

	public AdminCollectionAttribute(string _Path, string _Mask) : base(_Path)
	{
		Mask = _Mask;
	}
}