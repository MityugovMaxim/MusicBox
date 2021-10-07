using System;
using TMPro;
using UnityEngine;

public class UICoinsLabel : UIEntity
{
	const string COINS_ICON = "coins_icon";

	public long Coins
	{
		get => m_Coins;
		set
		{
			if (m_Coins == value)
				return;
			
			m_Coins = value;
			
			ProcessCoins();
		}
	}

	[SerializeField] TMP_Text m_Label;
	[SerializeField] bool     m_Sign;
	[SerializeField] long     m_Coins;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessCoins();
	}
	#endif

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessCoins();
	}

	void ProcessCoins()
	{
		string sign = m_Sign ? Coins >= 0 ? "+" : "-" : string.Empty;
		
		m_Label.text = $"{sign}{Math.Abs(Coins)}<sprite name=\"{COINS_ICON}\">";
	}
}