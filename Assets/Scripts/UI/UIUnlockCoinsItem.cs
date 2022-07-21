using UnityEngine;
using UnityEngine.Scripting;

public class UIUnlockCoinsItem : UIUnlockItem
{
	[Preserve]
	public class Pool : UIEntityPool<UIUnlockCoinsItem> { }

	[SerializeField] WebGraphic  m_Image;
	[SerializeField] UIUnitLabel m_Coins;

	public void Setup(long _Coins)
	{
		m_Image.Path  = "Thumbnails/Coins/coins_1.jpg";
		m_Coins.Value = _Coins;
		
		Restore();
	}
}