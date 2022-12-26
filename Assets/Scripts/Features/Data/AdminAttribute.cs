public class AdminAttribute
{
	public string Path { get; }

	protected AdminAttribute(string _Path)
	{
		Path = _Path;
	}
}