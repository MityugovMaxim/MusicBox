using System;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;

public class ProfileTransactions : ProfileCollection<ProfileTransaction>, IDataCollection
{
	public DataPriority Priority => DataPriority.Medium;

	protected override string Name => "transactions";

	readonly DataEventHandler m_PurchaseHandler = new DataEventHandler();

	public void SubscribePurchase(string _ProductID, Action _Action) => m_PurchaseHandler.AddListener(_ProductID, _Action);

	public void UnsubscribePurchase(string _ProductID, Action _Action) => m_PurchaseHandler.RemoveListener(_ProductID, _Action);

	public bool ContainsProduct(string _ProductID)
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Any(_Snapshot => _Snapshot.ProductID == _ProductID);
	}

	protected override void OnSnapshotAdd(string _TransactionID)
	{
		base.OnSnapshotAdd(_TransactionID);
		
		ProfileTransaction snapshot = GetSnapshot(_TransactionID);
		
		if (snapshot == null)
			return;
		
		m_PurchaseHandler.Invoke(snapshot.ProductID);
	}

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile transactions loaded.");
		
		return base.OnLoad();
	}
}