using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class Mixer : MonoBehaviour
{
	[SerializeField] AudioSource        m_AudioSource;
	[SerializeField] AudioMixerGroup    m_MasterGroup;
	[SerializeField] AudioMixerGroup    m_HighpassGroup;
	[SerializeField] AudioMixerSnapshot m_HighpassDisabledSnapshot;
	[SerializeField] AudioMixerSnapshot m_HighpassEnabledSnapshot;

	[Inject] ScoreController m_ScoreController;

	CancellationTokenSource m_TokenSource;

	void Awake()
	{
		m_ScoreController.OnMiss.AddListener(OnMiss);
		m_ScoreController.OnFail.AddListener(OnFail);
		m_ScoreController.OnHit.AddListener(OnHit);
	}

	void OnDestroy()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_ScoreController.OnMiss.RemoveListener(OnMiss);
		m_ScoreController.OnFail.RemoveListener(OnFail);
		m_ScoreController.OnHit.RemoveListener(OnHit);
	}

	void OnMiss()
	{
		RegisterMiss();
	}

	void OnFail()
	{
		RegisterMiss();
	}

	void OnHit()
	{
		RegisterHit();
	}

	async void RegisterMiss()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			m_AudioSource.outputAudioMixerGroup = m_HighpassGroup;
			
			m_HighpassEnabledSnapshot.TransitionTo(0);
			
			await Task.Delay(800, token);
			
			m_HighpassDisabledSnapshot.TransitionTo(0.2f);
			
			await Task.Delay(300, token);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_AudioSource.outputAudioMixerGroup = m_MasterGroup;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void RegisterHit()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_AudioSource.outputAudioMixerGroup = m_MasterGroup;
	}
}
