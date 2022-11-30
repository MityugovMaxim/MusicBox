using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProfileCoinsParameter : ProfileParameter<long>, IDataObject
{
	protected override string Name => "coins";

	[Inject] ProductsManager m_ProductsManager;
	[Inject] MenuProcessor   m_MenuProcessor;

	public async Task<bool> Remove(long _Coins)
	{
		if (Value >= _Coins)
		{
			Value -= _Coins;
			return true;
		}
		
		long coins = _Coins - Value;
		
		string productID = m_ProductsManager.GetProductID(coins);
		
		if (string.IsNullOrEmpty(productID))
			return false;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		return false;
	}

	protected override long Create(DataSnapshot _Data)
	{
		long coins = _Data.GetLong();
		
		return coins;
	}
}
