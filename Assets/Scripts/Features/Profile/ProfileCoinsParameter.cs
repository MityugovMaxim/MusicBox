using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public class ProfileDailyData : Snapshot
{
	public string DailyID        { get; }
	public long   StartTimestamp { get; }
	public long   EndTimestamp   { get; }

	public ProfileDailyData(DataSnapshot _Data) : base(_Data)
	{
		DailyID        = _Data.GetString("daily_id");
		StartTimestamp = _Data.GetLong("start_timestamp");
		EndTimestamp   = _Data.GetLong("end_timestamp");
	}
}

[Preserve]
public class ProfileDaily : ProfileParameter<ProfileDailyData>, IDataObject
{
	protected override string Name => "daily";

	protected override ProfileDailyData Create(DataSnapshot _Data) => new ProfileDailyData(_Data);
}

[Preserve]
public class ProfileCoinsParameter : ProfileParameter<long>, IDataObject
{
	protected override string Name => "coins";

	[Inject] ProductsManager m_ProductsManager;
	[Inject] MenuProcessor   m_MenuProcessor;

	public Task<bool> ReduceAsync(long _Coins)
	{
		if (Value >= _Coins)
		{
			Value -= _Coins;
			return Task.FromResult(true);
		}
		
		long coins = _Coins - Value;
		
		string productID = m_ProductsManager.GetProductID(coins);
		
		if (string.IsNullOrEmpty(productID))
			return Task.FromResult(false);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(productID);
		
		TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
		
		productMenu.Setup(productID, _Success => task.TrySetResult(_Success));
		productMenu.Show();
		
		return task.Task;
	}

	protected override long Create(DataSnapshot _Data)
	{
		long coins = _Data.GetLong();
		
		return coins;
	}
}
