using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIOfferAction : UIEntity
{
	public string OfferID { get; set; }

	[SerializeField] Button m_Button;

	[Inject] OffersManager m_OffersManager;
	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(Process);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(Process);
	}

	async void Process()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_OffersManager.Collect(OfferID);
		}
		catch (UnityException)
		{
			await m_MenuProcessor.ErrorAsync("offer_collect");
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}
