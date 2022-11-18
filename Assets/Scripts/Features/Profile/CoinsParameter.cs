using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class CoinsParameter : ProfileParameter<long>
{
	protected override string Name => $"coins";

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
}