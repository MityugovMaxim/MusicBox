using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public enum VoucherType
{
	None            = 0,
	ProductDiscount = 1,
	SongDiscount    = 2,
}

public enum VoucherGroup
{
	All        = 0,
	F2P        = 1,
	Purchasers = 2,
}

[Preserve]
public class VoucherSnapshot : Snapshot
{
	public VoucherType           Type       { get; }
	public VoucherGroup          Group      { get; }
	public double                Amount     { get; }
	public bool                  Available  { get; }
	public long                  Expiration { get; }
	public IReadOnlyList<string> IDs        { get; }

	public VoucherSnapshot(string _ID, int _Order) : base(_ID, _Order)
	{
		Type       = VoucherType.ProductDiscount;
		Group      = VoucherGroup.All;
		Available  = true;
		Expiration = 0;
		IDs        = new List<string>();
	}

	public VoucherSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type       = _Data.GetEnum<VoucherType>("type");
		Group      = _Data.GetEnum<VoucherGroup>("group");
		Amount     = _Data.GetDouble("amount");
		Available  = _Data.GetBool("available");
		Expiration = _Data.GetLong("expiration");
		IDs        = _Data.GetChildKeys("ids");
	}
}

[Preserve]
public class VouchersDataUpdateSignal { }

public class VoucherTimerSignal
{
	public string      VoucherID   { get; }
	public VoucherType VoucherType { get; }

	public VoucherTimerSignal(string _VoucherID, VoucherType _VoucherType)
	{
		VoucherID   = _VoucherID;
		VoucherType = _VoucherType;
	}
}

[Preserve]
public class VouchersProcessor : DataProcessor<VoucherSnapshot, VouchersDataUpdateSignal>
{
	protected override string Path => $"profiles/{m_SocialProcessor.UserID}/vouchers";

	protected override bool SupportsDevelopment => false;

	[Inject] SocialProcessor m_SocialProcessor;

	CancellationTokenSource m_TokenSource;

	DatabaseReference m_Registry;

	protected override Task OnLoad()
	{
		if (m_Registry == null)
		{
			m_Registry              =  FirebaseDatabase.DefaultInstance.RootReference.Child("vouchers");
			m_Registry.ChildAdded   += ProcessVouchers;
			m_Registry.ChildRemoved += ProcessVouchers;
			m_Registry.ChildMoved   += ProcessVouchers;
		}
		
		return base.OnLoad();
	}

	protected override Task OnUpdate()
	{
		ProcessTimers();
		
		return base.OnUpdate();
	}

	protected override Task OnFetch()
	{
		ProcessTimers();
		
		return base.OnFetch();
	}

	public string GetVoucherID(VoucherType _VoucherType, string _ID = null)
	{
		if (_VoucherType == VoucherType.None)
			return null;
		
		long timestamp = TimeUtility.GetTimestamp();
		
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Type == _VoucherType)
			.Where(_Snapshot => _Snapshot.Available)
			.Where(_Snapshot => _Snapshot.Expiration == 0 || _Snapshot.Expiration >= timestamp)
			.Where(_Snapshot => _Snapshot.IDs == null || _Snapshot.IDs.Count == 0 || !string.IsNullOrEmpty(_ID) && _Snapshot.IDs.Contains(_ID))
			.OrderByDescending(_Snapshot => _Snapshot.Amount)
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault();
	}

	public VoucherType GetType(string _VoucherID)
	{
		VoucherSnapshot snapshot = GetSnapshot(_VoucherID);
		
		return snapshot?.Type ?? VoucherType.None;
	}

	public long GetValue(VoucherType _VoucherType, long _Value) => GetValue(_VoucherType, null, _Value);

	public long GetValue(VoucherType _VoucherType, string _ID, long _Value)
	{
		string voucherID = GetVoucherID(_VoucherType, _ID);
		
		if (string.IsNullOrEmpty(voucherID))
			return _Value;
		
		double amount = GetAmount(voucherID);
		
		return _Value + (long)(amount / 100 * _Value);
	}

	public double GetAmount(string _VoucherID)
	{
		VoucherSnapshot snapshot = GetSnapshot(_VoucherID);
		
		return snapshot?.Amount ?? 0;
	}

	public long GetExpiration(string _VoucherID)
	{
		VoucherSnapshot snapshot = GetSnapshot(_VoucherID);
		
		return snapshot?.Expiration ?? 0;
	}

	void ProcessTimers()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		long timestamp = TimeUtility.GetTimestamp();
		
		foreach (VoucherSnapshot snapshot in Snapshots)
		{
			if (snapshot == null || !snapshot.Available || snapshot.Expiration == 0 || snapshot.Expiration < timestamp)
				continue;
			
			string      voucherID   = snapshot.ID;
			VoucherType voucherType = snapshot.Type;
			
			int timer = (int)(snapshot.Expiration - timestamp) + 1000;
			
			Task.Delay(timer, m_TokenSource.Token).Dispatch(
				_Task =>
				{
					if (_Task.IsCompletedSuccessfully)
						SignalBus.Fire(new VoucherTimerSignal(voucherID, voucherType));
				}
			);
		}
	}

	async void ProcessVouchers(object _Sender, ChildChangedEventArgs _Args)
	{
		VoucherLibraryRequest request = new VoucherLibraryRequest();
		
		await request.SendAsync();
	}
}
