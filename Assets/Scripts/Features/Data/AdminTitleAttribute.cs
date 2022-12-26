public class AdminTitleAttribute : AdminAttribute
{
	public string Title { get; }

	public AdminTitleAttribute(string _Path, string _Title) : base(_Path)
	{
		Title = _Title;
	}
}
