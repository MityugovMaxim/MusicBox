using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class SoundProcessor
{
	readonly SoundSource.Pool                m_SourcePool;
	readonly Dictionary<string, SoundInfo>   m_SoundInfos;
	readonly Dictionary<string, AudioClip>   m_SoundCache;
	readonly Dictionary<string, SoundSource> m_SoundSources;

	[Inject]
	public SoundProcessor(SoundSource.Pool _SourcePool, SoundInfo[] _SoundInfos)
	{
		m_SourcePool   = _SourcePool;
		m_SoundInfos   = _SoundInfos.ToDictionary(_SoundInfo => _SoundInfo.ID, _SoundInfo => _SoundInfo);
		m_SoundCache   = new Dictionary<string, AudioClip>();
		m_SoundSources = new Dictionary<string, SoundSource>();
	}

	public void Start(string _SoundID)
	{
		Stop(_SoundID);
		
		AudioClip sound = GetSound(_SoundID);
		
		if (sound == null)
		{
			Log.Error(this, "Start failed. Sound is null.");
			return;
		}
		
		SoundSource source = m_SourcePool.Spawn();
		
		if (source == null)
		{
			Log.Error(this, "Start failed. Source is null.");
			return;
		}
		
		m_SoundSources[_SoundID] = source;
		
		source.StartSound(sound);
	}

	public void Stop(string _SoundID)
	{
		if (!m_SoundSources.ContainsKey(_SoundID))
			return;
		
		SoundSource source = m_SoundSources[_SoundID];
		
		m_SoundSources.Remove(_SoundID);
		
		if (source == null)
		{
			Log.Error(this, "Stop failed. Source is null.");
			return;
		}
		
		source.StopSound();
	}

	public async void Play(string _SoundID)
	{
		await PlayAsync(_SoundID);
	}

	public async Task PlayAsync(string _SoundID)
	{
		AudioClip sound = GetSound(_SoundID);
		
		if (sound == null)
			return;
		
		SoundSource source = m_SourcePool.Spawn();
		
		await source.Play(sound);
		
		m_SourcePool.Despawn(source);
	}

	public AudioClip GetSound(string _SoundID)
	{
		if (string.IsNullOrEmpty(_SoundID))
		{
			Debug.LogError("[SoundProcessor] Get sound failed. Sound ID is null or empty.");
			return null;
		}
		
		if (m_SoundCache.ContainsKey(_SoundID) && m_SoundCache[_SoundID] != null)
			return m_SoundCache[_SoundID];
		
		if (!m_SoundInfos.ContainsKey(_SoundID))
		{
			Debug.LogErrorFormat("[SoundProcessor] Get sound failed. Sound '{0}' not found.", _SoundID);
			return null;
		}
		
		SoundInfo soundInfo = m_SoundInfos[_SoundID];
		
		if (soundInfo == null)
		{
			Debug.LogErrorFormat("[SoundProcessor] Get sound failed. Sound with ID '{0}' is null.", _SoundID);
			return null;
		}
		
		AudioClip sound = Resources.Load<AudioClip>(soundInfo.Path);
		
		if (sound == null)
			return null;
		
		m_SoundCache[_SoundID] = sound;
		
		return sound;
	}
}

[Preserve]
public class HapticProcessor : IInitializable, IDisposable
{
	const string HAPTIC_ENABLED_KEY = "HAPTIC_ENABLED";

	public bool HapticSupported => m_Haptic.SupportsHaptic;

	public bool HapticEnabled
	{
		get => m_HapticEnabled;
		set
		{
			if (m_HapticEnabled == value)
				return;
			
			m_HapticEnabled = value;
			
			PlayerPrefs.SetInt(HAPTIC_ENABLED_KEY, m_HapticEnabled ? 1 : 0);
		}
	}

	SignalBus m_SignalBus;
	Haptic    m_Haptic;
	bool      m_HapticEnabled;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus     = _SignalBus;
		m_Haptic        = Haptic.Create();
		m_HapticEnabled = PlayerPrefs.GetInt(HAPTIC_ENABLED_KEY, 1) > 0;
	}

	public void Process(Haptic.Type _HapticType)
	{
		if (m_HapticEnabled)
			m_Haptic.Process(_HapticType);
	}

	public async void Play(Haptic.Type _HapticType, int _Frequency, float _Duration)
	{
		await UnityTask.Tick(() => Process(_HapticType), _Frequency, _Duration);
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<DoubleSuccessSignal>(ImpactHeavy);
		m_SignalBus.Subscribe<HoldSuccessSignal>(ImpactMedium);
		m_SignalBus.Subscribe<TapSuccessSignal>(ImpactMedium);
		m_SignalBus.Subscribe<HoldHitSignal>(ImpactLight);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(ImpactHeavy);
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(ImpactMedium);
		m_SignalBus.Unsubscribe<TapSuccessSignal>(ImpactMedium);
		m_SignalBus.Unsubscribe<HoldHitSignal>(ImpactLight);
	}

	void ImpactHeavy()
	{
		Process(Haptic.Type.ImpactHeavy);
	}

	void ImpactMedium()
	{
		Process(Haptic.Type.ImpactMedium);
	}

	void ImpactLight()
	{
		Process(Haptic.Type.ImpactLight);
	}
}
