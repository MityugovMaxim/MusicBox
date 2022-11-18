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
		m_ScoreController.OnComboChange += OnComboChanged;
	}

	void OnDestroy()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_ScoreController.OnComboChange -= OnComboChanged;
	}

	void OnComboChanged(int _Combo, ScoreGrade _ScoreGrade)
	{
		if (_ScoreGrade == ScoreGrade.Fail || _ScoreGrade == ScoreGrade.Miss)
			RegisterMiss();
		else
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
