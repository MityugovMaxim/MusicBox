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
		
		m_AmbientManager.SubscribeTrack(ProcessLabel);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_AmbientManager.UnsubscribeTrack(ProcessLabel);
	}

	async void ProcessLabel()
	{
		await m_AmbientManager.Activate();
		
		if (!IsActiveSelf)
			return;
		
		m_Title.text  = m_AmbientManager.GetTitle();
		m_Artist.text = m_AmbientManager.GetArtist();
	}
}
