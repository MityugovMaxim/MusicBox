using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class StoreManager : IDataManager
{
	public StoreCollection Collection => m_StoreCollection;

	[Inject] StoreCollection m_StoreCollection;
	[Inject] StoreProcessor  m_StoreProcessor;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			TaskProvider.Group(
				m_StoreCollection.Load
			),
			TaskProvider.Group(
				ProcessProducts
			)
		);
	}

	public string GetPriceSign(string _StoreID) => m_StoreProcessor.GetPrice(_StoreID);

	public string GetPriceCode(string _StoreID) => m_StoreProcessor.GetPrice(_StoreID, false);

	public Task<RequestState> Purchase(string _StoreID, FunctionRequest<bool> _Request)
	{
		return m_StoreProcessor.Purchase(_StoreID, _Request);
	}

	List<string> GetStoreIDs() => Collection.GetIDs().ToList();

	IDs GetStoreIdentifiers(string _StoreID)
	{
		StoreSnapshot snapshot = Collection.GetSnapshot(_StoreID);
		
		if (snapshot == null)
			return null;
		
		return new IDs()
		{
			{ snapshot.AppStoreID, AppleAppStore.Name },
			{ snapshot.GooglePlayID, GooglePlay.Name },
		};
	}

	Task ProcessProducts()
	{
		m_StoreProcessor.Clear();
		
		List<string> storeIDs = GetStoreIDs();
		
		if (storeIDs == null || storeIDs.Count == 0)
			return Task.CompletedTask;
		
		foreach (string storeID in storeIDs)
			m_StoreProcessor.Add(storeID, GetStoreIdentifiers(storeID));
		
		return m_StoreProcessor.Load();
	}
}
