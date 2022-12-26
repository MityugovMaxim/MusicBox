using UnityEngine;
using Zenject;

public class UIOfferAction : UIOfferEntity
{
	[SerializeField] UIOverlayButton m_Button;

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
			await OffersManager.Collect(OfferID);
		}
		catch (UnityException)
		{
			await m_MenuProcessor.ErrorAsync("offer_collect");
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }
}
