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

	[Inject] SignalBus m_SignalBus;

	CancellationTokenSource m_TokenSource;

	void Awake()
	{
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHit);
		m_SignalBus.Subscribe<HoldHitSignal>(RegisterHit);
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterHit);
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterHit);
		
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterMiss);
		m_SignalBus.Subscribe<HoldMissSignal>(RegisterMiss);
		m_SignalBus.Subscribe<TapFailSignal>(RegisterMiss);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterMiss);
	}

	void OnDestroy()
	{
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHit);
		m_SignalBus.Unsubscribe<HoldHitSignal>(RegisterHit);
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterHit);
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterHit);
		
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterMiss);
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterMiss);
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterMiss);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterMiss);
	}

	void RegisterHit()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_AudioSource.outputAudioMixerGroup = m_MasterGroup;
	}

	async void RegisterMiss()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
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
}
