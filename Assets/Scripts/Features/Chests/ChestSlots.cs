using System;
using System.Collections.Generic;
using System.Linq;

public class ChestSlots : ProfileCollection<ChestSlot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Name => "chests/slots";

	readonly DataEventHandler<int> m_AddHandler    = new DataEventHandler<int>();
	readonly DataEventHandler<int> m_RemoveHandler = new DataEventHandler<int>();
	readonly DataEventHandler<int> m_ChangeHandler = new DataEventHandler<int>();
	readonly DataEventHandler<int> m_ReorderHandler = new DataEventHandler<int>();

	public List<int> GetSlots()
	{
		return Snapshots
			.Select(_Snapshot => _Snapshot.Slot)
			.ToList();
	}

	public ChestSlot GetSnapshot(int _Slot)
	{
		return Snapshots.FirstOrDefault(_Snapshot => _Snapshot.Slot == _Slot);
	}

	public void Subscribe(DataEventType _EventType, int _Slot, Action _Action) => GetHandler(_EventType)?.AddListener(_Slot, _Action);

	public void Unsubscribe(DataEventType _EventType, int _Slot, Action _Action) => GetHandler(_EventType)?.RemoveListener(_Slot, _Action);

	public void Subscribe(DataEventType _EventType, int _Slot, Action<int> _Action) => GetHandler(_EventType)?.AddListener(_Slot, _Action);

	public void Unsubscribe(DataEventType _EventType, int _Slot, Action<int> _Action) => GetHandler(_EventType)?.RemoveListener(_Slot, _Action);

	protected override void OnSnapshotAdd(string _SlotID)
	{
		base.OnSnapshotAdd(_SlotID);
		
		InvokeHandler(DataEventType.Add, _SlotID);
	}

	protected override void OnSnapshotRemove(string _SlotID)
	{
		base.OnSnapshotRemove(_SlotID);
		
		InvokeHandler(DataEventType.Remove, _SlotID);
	}

	protected override void OnSnapshotChange(string _SlotID)
	{
		base.OnSnapshotChange(_SlotID);
		
		InvokeHandler(DataEventType.Change, _SlotID);
	}

	protected override void OnSnapshotReorder(string _SlotID)
	{
		base.OnSnapshotReorder(_SlotID);
		
		InvokeHandler(DataEventType.Reorder, _SlotID);
	}

	DataEventHandler<int> GetHandler(DataEventType _EventType)
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

	void InvokeHandler(DataEventType _EventType, string _SlotID)
	{
		DataEventHandler<int> handler = GetHandler(_EventType);
		
		if (handler == null)
			return;
		
		ChestSlot snapshot = GetSnapshot(_SlotID);
		
		if (snapshot == null)
			return;
		
		handler.Invoke(snapshot.Slot);
	}
}
