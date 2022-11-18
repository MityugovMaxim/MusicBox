using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIOfferItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIOfferItem> { }

	[SerializeField] UIOfferImage m_Image;
	[SerializeField] UIOfferLabel m_Label;
	[SerializeField] UIOfferState m_State;
	[SerializeField] Button       m_CollectButton;

	[Inject] OffersManager m_OffersManager;
	[Inject] MenuProcessor m_MenuProcessor;

	string m_OfferID;

	protected override void Awake()
	{
		base.Awake();
		
		m_CollectButton.Subscribe(Collect);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_CollectButton.Unsubscribe(Collect);
	}

	public void Setup(string _OfferID)
	{
		m_OfferID = _OfferID;
		
		m_Image.OfferID = m_OfferID;
		m_Label.OfferID = m_OfferID;
		m_State.OfferID = m_OfferID;
	}

	async void Collect()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_OffersManager.Collect(m_OfferID);
		}
		catch (UnityException)
		{
			await m_MenuProcessor.ErrorAsync("offer_collect");
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}
