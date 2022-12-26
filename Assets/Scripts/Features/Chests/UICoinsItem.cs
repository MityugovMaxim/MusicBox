using UnityEngine;

public class UICoinsItem : UIEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	public void Setup(long _Coins) => m_Coins.Value = _Coins;

	public void Setup(double _Coins) => m_Coins.Value = _Coins;
}
