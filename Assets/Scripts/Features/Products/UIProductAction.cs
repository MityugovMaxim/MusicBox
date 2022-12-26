using UnityEngine;
using Zenject;

public class UIProductAction : UIProductEntity
{
	[SerializeField] UIFlare  m_Flare;
	[SerializeField] UIGroup  m_LoaderGroup;
	[SerializeField] UIButton m_Button;

	[SerializeField, Sound] string      m_Sound;
	[SerializeField]        Haptic.Type m_Haptic;

	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(ProcessAction);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(ProcessAction);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }

	async void ProcessAction()
	{
		m_LoaderGroup.Show();
		
		RequestState state = await ProductsManager.Purchase(ProductID);
		
		m_LoaderGroup.Hide();
		
		if (state == RequestState.Fail)
		{
			await m_MenuProcessor.ErrorAsync("product_purchase");
		}
		else if (state == RequestState.Success)
		{
			m_Flare.Play();
			m_SoundProcessor.Play(m_Sound);
			m_HapticProcessor.Process(m_Haptic);
		}
	}
}
