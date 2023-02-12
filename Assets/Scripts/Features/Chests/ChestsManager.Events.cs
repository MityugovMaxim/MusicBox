using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zenject;

public partial class ChestsManager : IInitializable, IDisposable
{
	[Inject] ScheduleProcessor m_ScheduleProcessor;

	readonly DataEventHandler<int> m_StartTimer  = new DataEventHandler<int>();
	readonly DataEventHandler<int> m_EndTimer    = new DataEventHandler<int>();
	readonly DataEventHandler<int> m_CancelTimer = new DataEventHandler<int>();

	void IInitializable.Initialize()
	{
		Slots.Subscribe(DataEventType.Add, ProcessTimer);
		Slots.Subscribe(DataEventType.Remove, ProcessTimer);
		Slots.Subscribe(DataEventType.Change, ProcessTimer);
	}

	void IDisposable.Dispose()
	{
		Slots.Unsubscribe(DataEventType.Add, ProcessTimer);
		Slots.Unsubscribe(DataEventType.Remove, ProcessTimer);
		Slots.Unsubscribe(DataEventType.Change, ProcessTimer);
	}

	public void SubscribeStartTimer(Action _Action) => m_StartTimer.AddListener(_Action);

	public void UnsubscribeStartTimer(Action _Action) => m_StartTimer.RemoveListener(_Action);
	
	public void SubscribeEndTimer(Action _Action) => m_EndTimer.AddListener(_Action);

	public void UnsubscribeEndTimer(Action _Action) => m_EndTimer.RemoveListener(_Action);

	public void SubscribeCancelTimer(Action _Action) => m_CancelTimer.AddListener(_Action);

	public void UnsubscribeCancelTimer(Action _Action) => m_CancelTimer.RemoveListener(_Action); 

	public void SubscribeStartTimer(int _Slot, Action _Action) => m_StartTimer.AddListener(_Slot, _Action);

	public void UnsubscribeStartTimer(int _Slot, Action _Action) => m_StartTimer.RemoveListener(_Slot, _Action);

	public void SubscribeEndTimer(int _Slot, Action _Action) => m_EndTimer.AddListener(_Slot, _Action);

	public void UnsubscribeEndTimer(int _Slot, Action _Action) => m_EndTimer.RemoveListener(_Slot, _Action);

	public void SubscribeCancelTimer(int _Slot, Action _Action) => m_CancelTimer.AddListener(_Slot, _Action);

	public void UnsubscribeCancelTimer(int _Slot, Action _Action) => m_CancelTimer.RemoveListener(_Slot, _Action);

	Task ProcessTimers()
	{
		IReadOnlyList<string> slotIDs = Slots.GetIDs();
		if (slotIDs != null && slotIDs.Count != 0)
		{
			foreach (string slotID in slotIDs)
				ProcessTimer(slotID);
		}
		return Task.CompletedTask;
	}

	void ProcessTimer(string _SlotID)
	{
		ChestSlot snapshot = Slots.GetSnapshot(_SlotID);
		
		if (snapshot == null)
			return;
		
		long timestamp = TimeUtility.GetTimestamp();
		
		if (timestamp >= snapshot.StartTimestamp && timestamp >= snapshot.EndTimestamp)
			return;
		
		m_ScheduleProcessor.Schedule(
			this,
			_SlotID,
			snapshot.Slot,
			snapshot.StartTimestamp,
			snapshot.EndTimestamp,
			m_StartTimer,
			m_EndTimer,
			m_CancelTimer
		);
	}

	public void SubscribeChests(RankType _Rank, Action _Action)
	{
		ProfileChests chests = GetProfileChests(_Rank);
		
		chests?.Subscribe(_Action);
	}

	public void UnsubscribeChests(RankType _Rank, Action _Action)
	{
		ProfileChests chests = GetProfileChests(_Rank);
		
		chests?.Unsubscribe(_Action);
	}

	ProfileChests GetProfileChests(RankType _Rank)
	{
		switch (_Rank)
		{
			case RankType.Bronze:   return m_ProfileBronzeChests;
			case RankType.Silver:   return m_ProfileSilverChests;
			case RankType.Gold:     return m_ProfileGoldChests;
			case RankType.Platinum: return m_ProfilePlatinumChests;
			default:                return null;
		}
	}
}
