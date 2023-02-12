using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ReviveSnapshot : Snapshot
{
	public int  Count  { get; }
	public long Coins  { get; }

	public ReviveSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Count = _Data.GetInt("count");
		Coins = _Data.GetLong("coins");
	}
}

[Preserve]
public class RevivesManager : IDataManager
{
	public RevivesCollection Collection => m_RevivesCollection;

	[Inject] RevivesCollection m_RevivesCollection;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			Collection.Load
		);
	}

	public long GetCoins(int _Count)
	{
		ReviveSnapshot snapshot = Collection.GetSnapshot(_Count);
		
		return snapshot?.Coins ?? 0;
	}
}

[Preserve]
public class RevivesCollection : DataCollection<ReviveSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.High;

	protected override string Path => "revives";

	public ReviveSnapshot GetSnapshot(int _Count)
	{
		return Snapshots.ApproximatelyMax(_Snapshot => _Snapshot.Count, _Count);
	}
}
