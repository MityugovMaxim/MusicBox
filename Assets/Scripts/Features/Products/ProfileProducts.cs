using System;
using System.Threading.Tasks;
using AudioBox.Logging;

public class ProfileProducts : ProfileCollection<ProfileProduct>, IDataCollection
{
	public DataPriority Priority => DataPriority.Medium;

	protected override string Name => "products";

	public void SubscribePurchase(string _ProductID, Action _Action) => Subscribe(DataEventType.Add, _ProductID, _Action);

	public void UnsubscribePurchase(string _ProductID, Action _Action) => Unsubscribe(DataEventType.Add, _ProductID, _Action);

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile transactions loaded.");
		
		return base.OnLoad();
	}
}
