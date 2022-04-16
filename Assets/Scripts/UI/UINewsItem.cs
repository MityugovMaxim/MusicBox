using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UINewsItem : UIGroupLayout
{
	[Preserve]
	public class Pool : UIEntityPool<UINewsItem>
	{
		protected override void OnDespawned(UINewsItem _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Hide(true);
		}
	}

	[SerializeField] UINewsImage m_Image;
	[SerializeField] TMP_Text    m_Title;
	[SerializeField] TMP_Text    m_Description;
	[SerializeField] TMP_Text    m_Date;

	[Inject] NewsProcessor      m_NewsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] UrlProcessor       m_UrlProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_NewsID;
	string m_URL;

	public void Setup(string _NewsID)
	{
		m_NewsID = _NewsID;
		
		m_Image.Setup(m_NewsID);
		
		m_Title.text   = m_NewsProcessor.GetTitle(m_NewsID);
		m_Description.text = m_NewsProcessor.GetDescription(m_NewsID);
		m_Date.text    = m_NewsProcessor.GetDate(m_NewsID);
		m_URL          = m_NewsProcessor.GetURL(m_NewsID);
	}

	public async void Open()
	{
		m_StatisticProcessor.LogMainMenuNewsPageItemClick(m_NewsID);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_UrlProcessor.ProcessURL(m_URL);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}
}