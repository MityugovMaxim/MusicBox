using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class LanguagesCollection : DataCollection<LanguageSnapshot>
{
	protected override string Path => "languages";

	protected override Task OnLoad()
	{
		Log.Info(this, "Languages loaded.");
		
		return base.OnLoad();
	}
}
