using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class VouchersManager : ProfileCollection<DataSnapshot<bool>>
{
	protected override string Name => "vouchers";

	[Inject] VouchersCollection m_Vouchers;
	[Inject] ProductsCollection m_Products;
	[Inject] SongsCollection    m_Songs;

	readonly DataEventHandler m_ExpirationHandler = new DataEventHandler(DataEventType.None);
	readonly DataEventHandler m_CancelHandler     = new DataEventHandler(DataEventType.None);

	readonly Dictionary<string, CancellationTokenSource> m_Timers = new Dictionary<string, CancellationTokenSource>();

	public void SubscribeExpiration(string _TimerID, Action _Action) => m_ExpirationHandler.AddListener(_TimerID, _Action);

	public void SubscribeCancel(string _TimerID, Action _Action) => m_CancelHandler.AddListener(_TimerID, _Action);

	public void UnsubscribeExpiration(string _TimerID, Action _Action) => m_ExpirationHandler.RemoveListener(_TimerID, _Action);

	public void UnsubscribeCancel(string _TimerID, Action _Action) => m_CancelHandler.RemoveListener(_TimerID, _Action);

	public long GetProductDiscount(string _ProductID)
	{
		ProductSnapshot snapshot = m_Products.GetSnapshot(_ProductID);
		
		string voucherID = GetProductVoucherID(_ProductID);
		
		long coins = snapshot?.Coins ?? 0;
		
		return GetDiscount(voucherID, coins);
	}

	public long GetSongDiscount(string _SongID)
	{
		SongSnapshot snapshot = m_Songs.GetSnapshot(_SongID);
		
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
		VoucherSnapshot snapshot = m_Vouchers.GetSnapshot(_VoucherID);
		
		return snapshot?.Amount ?? 0;
	}

	public long GetExpiration(string _VoucherID)
	{
		VoucherSnapshot snapshot = m_Vouchers.GetSnapshot(_VoucherID);
		
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
		
		return GetIDs()
			.Where(_VoucherID => !string.IsNullOrEmpty(_VoucherID))
			.Where(GetValue)
			.Select(m_Vouchers.GetSnapshot)
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Type == _VoucherType)
			.Where(_Snapshot => _Snapshot.IDs == null || _Snapshot.IDs.Count > 0 && _Snapshot.IDs.Contains(_ID))
			.Where(_Snapshot => _Snapshot.Expiration == 0 || _Snapshot.Expiration >= timestamp)
			.OrderByDescending(_Snapshot => Math.Abs(_Snapshot.Amount))
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault(GetValue);
	}

	bool GetValue(string _VoucherID)
	{
		DataSnapshot<bool> snapshot = GetSnapshot(_VoucherID);
		
		return snapshot?.Value ?? false;
	}

	void CancelTimer(string _VoucherID)
	{
		if (!m_Timers.TryGetValue(_VoucherID, out CancellationTokenSource tokenSource) || tokenSource == null)
			return;
		
		tokenSource.Cancel();
		tokenSource.Dispose();
		
		m_Timers.Remove(_VoucherID);
	}

	void CompleteTimer(string _VoucherID)
	{
		if (!m_Timers.TryGetValue(_VoucherID, out CancellationTokenSource tokenSource) || tokenSource == null)
			return;
		
		tokenSource.Dispose();
		
		m_Timers.Remove(_VoucherID);
		
		m_CancelHandler.Invoke(_VoucherID);
	}

	void ProcessTimer(string _VoucherID)
	{
		if (string.IsNullOrEmpty(_VoucherID))
			return;
		
		CancelTimer(_VoucherID);
		
		VoucherSnapshot snapshot = m_Vouchers.GetSnapshot(_VoucherID);
		
		if (snapshot == null)
			return;
		
		long timestamp = TimeUtility.GetTimestamp();
		
		long expiration = snapshot.Expiration;
		
		if (expiration > 0 && expiration < timestamp)
			return;
		
		int timer = (int)(expiration - timestamp) + 1000;
		
		m_Timers[_VoucherID] = new CancellationTokenSource();
		
		Task.Delay(timer, m_Timers[_VoucherID].Token).Dispatch(
			_Task =>
			{
				CompleteTimer(_VoucherID);
				if (_Task.IsCompletedSuccessfully)
					m_ExpirationHandler.Invoke(_VoucherID);
			}
		);
	}
}
