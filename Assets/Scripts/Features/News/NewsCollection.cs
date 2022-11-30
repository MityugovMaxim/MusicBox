using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class NewsCollection : DataCollection<NewsSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "news";

	protected override Task OnLoad()
	{
		Log.Info(this, "News loaded.");
		
		return base.OnLoad();
	}
}
