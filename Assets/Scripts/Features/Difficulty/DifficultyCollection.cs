using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class DifficultyCollection : DataCollection<DifficultySnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Medium;

	protected override string Path => "difficulty";

	public DifficultySnapshot GetSnapshot(RankType _SongRank)
	{
		return Snapshots.FirstOrDefault(_Snapshot => _Snapshot.Type == _SongRank);
	}

	protected override Task OnLoad()
	{
		Log.Info(this, "Difficulty loaded.");
		
		return base.OnLoad();
	}
}
