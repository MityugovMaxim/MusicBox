using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInterstitialItem : UIEntity
{
	public string BannerID => m_BannerID;

	[SerializeField] TMP_Text m_Label;
	[SerializeField] Button   m_OpenButton;
	[SerializeField] Button   m_RemoveButton;

	string         m_BannerID;
	Action<string> m_Open;
	Action<string> m_Remove;

	public void Setup(string _BannerID, Action<string> _Open, Action<string> _Remove)
	{
		m_BannerID = _BannerID;
		m_Open     = _Open;
		m_Remove   = _Remove;
		
		m_Label.text = !string.IsNullOrEmpty(BannerID) ? BannerID : "[EMPTY]";
		
		m_OpenButton.onClick.RemoveAllListeners();
		m_OpenButton.onClick.AddListener(Open);
		
		m_RemoveButton.onClick.RemoveAllListeners();
		m_RemoveButton.onClick.AddListener(Remove);
	}

	void Open()
	{
		m_Open?.Invoke(BannerID);
	}

	void Remove()
	{
		m_Remove?.Invoke(BannerID);
	}
}