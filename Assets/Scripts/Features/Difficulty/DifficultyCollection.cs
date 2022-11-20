using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class DifficultyCollection : DataCollection<DifficultySnapshot>, IDataCollection
{
	public DataCollectionPriority Priority => DataCollectionPriority.Medium;

	protected override string Path => "difficulty";

	public DifficultySnapshot GetSnapshot(DifficultyType _DifficultyType)
	{
		return Snapshots.FirstOrDefault(_Snapshot => _Snapshot.Type == _DifficultyType);
	}

	protected override Task OnLoad()
	{
		Log.Info(this, "Difficulty loaded.");
		
		return base.OnLoad();
	}
}
