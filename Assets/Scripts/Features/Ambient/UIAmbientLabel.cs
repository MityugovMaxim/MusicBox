using TMPro;
using UnityEngine;
using Zenject;

public class UIAmbientLabel : UIEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Artist;

	[Inject] AmbientManager m_AmbientManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessLabel();
		
		m_AmbientManager.SubscribePlay(ProcessLabel);
		m_AmbientManager.SubscribePause(ProcessLabel);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_AmbientManager.UnsubscribePlay(ProcessLabel);
		m_AmbientManager.UnsubscribePause(ProcessLabel);
	}

	async void ProcessLabel()
	{
		await m_AmbientManager.Activate();
		
		if (!IsActive)
			return;
		
		m_Title.text  = m_AmbientManager.GetTitle();
		m_Artist.text = m_AmbientManager.GetArtist();
	}
}
