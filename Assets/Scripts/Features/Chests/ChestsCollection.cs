using System;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class ChestsCollection : DataCollection<ChestSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "chests";

	readonly DataEventHandler<RankType> m_ChangeHandler  = new DataEventHandler<RankType>();
	readonly DataEventHandler<RankType> m_AddHandler     = new DataEventHandler<RankType>();
	readonly DataEventHandler<RankType> m_RemoveHandler  = new DataEventHandler<RankType>();
	readonly DataEventHandler<RankType> m_ReorderHandler = new DataEventHandler<RankType>();

	protected override Task OnLoad()
	{
		Log.Info(this, "Chests loaded.");
		
		return base.OnLoad();
	}

	public ChestSnapshot GetSnapshot(RankType _Rank)
	{
		return Snapshots.FirstOrDefault(_Snapshot => _Snapshot.Rank == _Rank);
	}

	protected override void OnSnapshotAdd(string _ChestID)
	{
		base.OnSnapshotAdd(_ChestID);
		
		InvokeHandler(DataEventType.Add, _ChestID);
	}

	protected override void OnSnapshotRemove(string _ChestID)
	{
		base.OnSnapshotRemove(_ChestID);
		
		InvokeHandler(DataEventType.Remove, _ChestID);
	}

	protected override void OnSnapshotChange(string _ChestID)
	{
		base.OnSnapshotChange(_ChestID);
		
		InvokeHandler(DataEventType.Change, _ChestID);
	}

	protected override void OnSnapshotReorder(string _ChestID)
	{
		base.OnSnapshotReorder(_ChestID);
		
		InvokeHandler(DataEventType.Reorder, _ChestID);
	}

	public void Subscribe(DataEventType _EventType, RankType _Rank, Action _Action) => GetHandler(_EventType)?.AddListener(_Rank, _Action);

	public void Unsubscribe(DataEventType _EventType, RankType _Rank, Action _Action) => GetHandler(_EventType)?.RemoveListener(_Rank, _Action);

	public void Subscribe(DataEventType _EventType, RankType _Rank, Action<RankType> _Action) => GetHandler(_EventType)?.AddListener(_Rank, _Action);
	
	public void Unsubscribe(DataEventType _EventType, RankType _Rank, Action<RankType> _Action) => GetHandler(_EventType)?.RemoveListener(_Rank, _Action);

	DataEventHandler<RankType> GetHandler(DataEventType _EventType)
	{
		switch (_EventType)
		{
			case DataEventType.Add:     return m_AddHandler;
			case DataEventType.Remove:  return m_RemoveHandler;
			case DataEventType.Change:  return m_ChangeHandler;
			case DataEventType.Reorder: return m_ReorderHandler;
			default:                    return null;
		}
	}

	void InvokeHandler(DataEventType _EventType, string _ChestID)
	{
		DataEventHandler<RankType> handler = GetHandler(_EventType);
		
		if (handler == null)
			return;
		
		ChestSnapshot snapshot = GetSnapshot(_ChestID);
		
		if (snapshot == null)
			return;
		
		handler.Invoke(snapshot.Rank);
	}
}
