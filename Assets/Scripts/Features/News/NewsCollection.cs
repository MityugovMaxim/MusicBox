using UnityEngine.Scripting;

[Preserve]
public class NewsCollection : DataCollection<NewsSnapshot>
{
	protected override string Path => "news";
}
