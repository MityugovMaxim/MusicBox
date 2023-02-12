using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public partial class VouchersManager : IDataManager, IInitializable, IDisposable
{
	public VouchersCollection Collection => m_VouchersCollection;

	public ProfileVouchers Profile => m_ProfileVouchers;

	[Inject] VouchersCollection m_VouchersCollection;
	[Inject] ProfileVouchers    m_ProfileVouchers;
	[Inject] ScheduleProcessor  m_ScheduleProcessor;

	readonly DataEventHandler m_StartHandler  = new DataEventHandler();
	readonly DataEventHandler m_CancelHandler = new DataEventHandler();
	readonly DataEventHandler m_EndHandler    = new DataEventHandler();

	void IInitializable.Initialize()
	{
		Collection.Subscribe(DataEventType.Add, Collect);
		Collection.Subscribe(DataEventType.Add, ProcessTimer);
		Collection.Subscribe(DataEventType.Remove, ProcessTimer);
		Collection.Subscribe(DataEventType.Change, ProcessTimer);
	}

	void IDisposable.Dispose()
	{
		Collection.Unsubscribe(DataEventType.Add, Collect);
		Collection.Unsubscribe(DataEventType.Add, ProcessTimer);
		Collection.Unsubscribe(DataEventType.Remove, ProcessTimer);
		Collection.Unsubscribe(DataEventType.Change, ProcessTimer);
	}

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			TaskProvider.Group(
				Collection.Load,
				Profile.Load
			),
			TaskProvider.Group(
				CollectVouchers,
				ProcessTimers
			)
		);
	}

	public List<string> GetVoucherIDs() => Profile.GetIDs().ToList();

	public VoucherType GetType(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.Type ?? VoucherType.None;
	}

	public long GetProductDiscount(string _ProductID, long _Value)
	{
		string voucherID = GetProductVoucherID(_ProductID);
		
		return GetDiscount(voucherID, _Value);
	}

	public long GetSongDiscount(string _SongID, long _Value)
	{
		string voucherID = GetSongVoucherID(_SongID);
		
		return GetDiscount(voucherID, _Value);
	}

	public long GetChestDiscount(string _SongID, long _Value)
	{
		string voucherID = GetChestVoucherID(_SongID);
		
		return GetDiscount(voucherID, _Value);
	}

	public long GetSeasonDiscount(string _SeasonID, long _Value)
	{
		string voucherID = GetSeasonVoucherID(_SeasonID);
		
		return GetDiscount(voucherID, _Value);
	}

	public string GetProductVoucherID(string _ProductID) => GetVoucherID(VoucherType.Product, _ProductID);

	public string GetSongVoucherID(string _SongID) => GetVoucherID(VoucherType.Song, _SongID);

	public string GetChestVoucherID(string _ChestID) => GetVoucherID(VoucherType.Chest, _ChestID);

	public string GetSeasonVoucherID(string _SeasonID) => GetVoucherID(VoucherType.Season, _SeasonID);

	public double GetAmount(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.Amount ?? 0;
	}

	public long GetStartTimestamp(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.EndTimestamp ?? 0;
	}

	public long GetEndTimestamp(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.EndTimestamp ?? 0;
	}

	public bool IsProcessing(string _VoucherID)
	{
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetStartTimestamp(_VoucherID);
		long endTimestamp   = GetEndTimestamp(_VoucherID);
		
		return timestamp >= startTimestamp && timestamp < endTimestamp;
	}

	long GetDiscount(string _VoucherID, long _Value)
	{
		if (string.IsNullOrEmpty(_VoucherID))
			return _Value;
		
		double amount = GetAmount(_VoucherID);
		
		return _Value + (long)(amount / 100 * _Value);
	}

	VoucherGroup GetGroup(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.Group ?? VoucherGroup.None;
	}

	string GetVoucherID(VoucherType _VoucherType, string _ID)
	{
		long timestamp = TimeUtility.GetTimestamp();
		
		return Profile.GetIDs()
			.Where(_VoucherID => !string.IsNullOrEmpty(_VoucherID))
			.Where(Profile.Contains)
			.Select(Collection.GetSnapshot)
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Type == _VoucherType)
			.Where(_Snapshot => _Snapshot.IDs == null || _Snapshot.IDs.Count > 0 && _Snapshot.IDs.Contains(_ID))
			.Where(_Snapshot => _Snapshot.EndTimestamp == 0 || _Snapshot.EndTimestamp >= timestamp)
			.OrderByDescending(_Snapshot => Math.Abs(_Snapshot.Amount))
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault();
	}

	async void Collect(string _VoucherID) => await CollectAsync(_VoucherID);

	Task CollectVouchers()
	{
		string[] voucherIDs = Collection.GetIDs()
			.Where(_VoucherID => GetGroup(_VoucherID) != VoucherGroup.None)
			.Where(_VoucherID => !Profile.Contains(_VoucherID))
			.ToArray();
		
		if (voucherIDs.Length == 0)
			return Task.CompletedTask;
		
		VouchersCollectRequest request = new VouchersCollectRequest(voucherIDs);
		
		return request.SendAsync();
	}

	Task ProcessTimers()
	{
		List<string> voucherIDs = GetVoucherIDs();
		
		if (voucherIDs != null && voucherIDs.Count != 0)
		{
			foreach (string voucherID in voucherIDs)
				ProcessTimer(voucherID);
		}
		
		return Task.CompletedTask;
	}

	Task CollectAsync(string _VoucherID)
	{
		if (string.IsNullOrEmpty(_VoucherID) || Profile.Contains(_VoucherID))
			return Task.CompletedTask;
		
		VoucherGroup group = GetGroup(_VoucherID);
		
		if (group == VoucherGroup.None)
			return Task.CompletedTask;
		
		VouchersCollectRequest request = new VouchersCollectRequest(_VoucherID);
		
		return request.SendAsync();
	}

	void ProcessTimer(string _VoucherID)
	{
		if (!IsProcessing(_VoucherID))
			return;
		
		long startTimestamp = GetStartTimestamp(_VoucherID);
		long endTimestamp   = GetEndTimestamp(_VoucherID);
		
		m_ScheduleProcessor.Schedule(
			this,
			_VoucherID,
			startTimestamp,
			endTimestamp,
			m_StartHandler,
			m_EndHandler,
			m_CancelHandler
		);
	}
}
