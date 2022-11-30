using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class VouchersManager : IDataManager, IInitializable, IDisposable
{
	public bool Activated { get; private set; }

	public VouchersCollection Collection => m_VouchersCollection;

	public ProfileVouchers Profile => m_ProfileVouchers;

	[Inject] VouchersCollection m_VouchersCollection;
	[Inject] ProfileVouchers    m_ProfileVouchers;
	[Inject] ProductsCollection m_ProductsCollection;
	[Inject] SongsCollection    m_SongsCollection;
	[Inject] ScheduleProcessor  m_ScheduleProcessor;

	readonly DataEventHandler m_ExpirationHandler = new DataEventHandler();
	readonly DataEventHandler m_CancelHandler     = new DataEventHandler();

	void IInitializable.Initialize()
	{
		Collection.Subscribe(DataEventType.Add, ProcessTimer);
		Collection.Subscribe(DataEventType.Remove, CancelTimer);
		Collection.Subscribe(DataEventType.Change, ProcessTimer);
	}

	void IDisposable.Dispose()
	{
		Collection.Unsubscribe(DataEventType.Add, ProcessTimer);
		Collection.Unsubscribe(DataEventType.Remove, CancelTimer);
		Collection.Unsubscribe(DataEventType.Change, ProcessTimer);
	}

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			m_VouchersCollection.Load(),
			m_ProfileVouchers.Load(),
			m_ProductsCollection.Load(),
			m_SongsCollection.Load()
		);
		
		Activated = true;
		
		return frame == Time.frameCount;
	}

	public void SubscribeExpiration(Action _Action) => m_ExpirationHandler.AddListener(_Action);
	public void SubscribeExpiration(string _VoucherID, Action _Action) => m_ExpirationHandler.AddListener(_VoucherID, _Action);

	public void SubscribeCancel(Action _Action) => m_CancelHandler.AddListener(_Action);
	public void SubscribeCancel(string _VoucherID, Action _Action) => m_CancelHandler.AddListener(_VoucherID, _Action);

	public void UnsubscribeExpiration(Action _Action) => m_ExpirationHandler.RemoveListener(_Action);
	public void UnsubscribeExpiration(string _VoucherID, Action _Action) => m_ExpirationHandler.RemoveListener(_VoucherID, _Action);

	public void UnsubscribeCancel(Action _Action) => m_CancelHandler.RemoveListener(_Action);
	public void UnsubscribeCancel(string _VoucherID, Action _Action) => m_CancelHandler.RemoveListener(_VoucherID, _Action);

	public long GetProductDiscount(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		string voucherID = GetProductVoucherID(_ProductID);
		
		long coins = snapshot?.Coins ?? 0;
		
		return GetDiscount(voucherID, coins);
	}

	public VoucherType GetType(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.Type ?? VoucherType.None;
	}

	public long GetSongDiscount(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		string voucherID = GetSongVoucherID(_SongID);
		
		long coins = snapshot?.Price ?? 0;
		
		return GetDiscount(voucherID, coins);
	}

	public long GetChestDiscount(string _SongID, long _Coins)
	{
		string voucherID = GetChestVoucherID(_SongID);
		
		return GetDiscount(voucherID, _Coins);
	}

	public string GetProductVoucherID(string _ProductID) => GetVoucherID(VoucherType.ProductDiscount, _ProductID);

	public string GetSongVoucherID(string _ProductID) => GetVoucherID(VoucherType.SongDiscount, _ProductID);

	public string GetChestVoucherID(string _ChestID) => GetVoucherID(VoucherType.ChestDiscount, _ChestID);

	public double GetAmount(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.Amount ?? 0;
	}

	public long GetExpiration(string _VoucherID)
	{
		VoucherSnapshot snapshot = Collection.GetSnapshot(_VoucherID);
		
		return snapshot?.Expiration ?? 0;
	}

	long GetDiscount(string _VoucherID, long _Value)
	{
		if (string.IsNullOrEmpty(_VoucherID))
			return _Value;
		
		double amount = GetAmount(_VoucherID);
		
		return _Value + (long)(amount / 100 * _Value);
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
			.Where(_Snapshot => _Snapshot.Expiration == 0 || _Snapshot.Expiration >= timestamp)
			.OrderByDescending(_Snapshot => Math.Abs(_Snapshot.Amount))
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault();
	}

	void CancelTimer(string _VoucherID) => m_ScheduleProcessor.Cancel(_VoucherID);

	void ProcessTimer(string _VoucherID) => m_ScheduleProcessor.Schedule(_VoucherID, GetExpiration(_VoucherID), m_ExpirationHandler, m_CancelHandler);
}
