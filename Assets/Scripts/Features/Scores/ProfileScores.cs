using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class ProfileScores : ProfileCollection<ProfileScore>, IDataCollection
{
	public DataPriority Priority => DataPriority.Medium;

	protected override string Name => "scores";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile scores loaded.");
		
		return base.OnLoad();
	}
}