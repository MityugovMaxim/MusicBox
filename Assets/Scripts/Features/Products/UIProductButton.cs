using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIProductButton : UIProductEntity
{
	public DynamicDelegate<bool> OnPurchase { get; } = new DynamicDelegate<bool>();

	[SerializeField] UIFlare m_Flare;
	[SerializeField] UIGroup m_ContentGroup;
	[SerializeField] UIGroup m_LoaderGroup;
	[SerializeField] Button  m_PurchaseButton;

	[SerializeField, Sound] string      m_Sound;
	[SerializeField]        Haptic.Type m_Haptic;

	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_PurchaseButton.Subscribe(Purchase);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_PurchaseButton.Unsubscribe(Purchase);
	}

	async void Purchase()
	{
		m_ContentGroup.Hide();
		m_LoaderGroup.Show();
		
		RequestState state = await ProductsManager.Purchase(ProductID);
		
		m_LoaderGroup.Hide();
		m_ContentGroup.Show();
		
		if (state == RequestState.Fail)
		{
			await m_MenuProcessor.RetryAsync(
				"product_purchase",
				Purchase,
				Cancel
			);
		}
		else if (state == RequestState.Success)
		{
			m_Flare.Play();
			m_SoundProcessor.Play(m_Sound);
			m_HapticProcessor.Process(m_Haptic);
			
			OnPurchase.Invoke(true);
			
			Complete();
		}
	}

	void Cancel()
	{
		OnPurchase.Invoke(false);
	}

	async void Complete()
	{
		OnPurchase.Invoke(true);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData()
	{
		m_LoaderGroup.Hide(true);
		m_ContentGroup.Show(true);
	}
}
