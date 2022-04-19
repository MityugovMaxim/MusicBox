using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AudioSourceChangedSignal { }

public enum AudioOutputType
{
	BuiltIn    = 0,
	Headphones = 1,
	Bluetooth  = 2,
	Unknown    = 3,
}

public abstract class AudioManager : IInitializable, IDisposable
{
	protected delegate void RemoteCommandHandler();

	const string LATENCY_KEY = "LATENCY";

	[Inject] SignalBus m_SignalBus;

	void IInitializable.Initialize()
	{
		Load(InvokeAudioSourceChanged);
	}

	void IDisposable.Dispose()
	{
		Unload();
	}

	protected abstract void Load(Action _AudioSourceChanged);
	protected abstract void Unload();

	public abstract void SetAudioActive(bool _Value);

	public abstract string GetAudioOutputName();

	public abstract AudioOutputType GetAudioOutputType();

	public abstract string GetAudioOutputID();

	public float GetLatency()
	{
		string key = $"{LATENCY_KEY}_{GetAudioOutputID()}";
		
		return PlayerPrefs.GetFloat(key, 0);
	}

	public void SetLatency(float _Latency)
	{
		string key = LATENCY_KEY + GetAudioOutputID();
		
		PlayerPrefs.SetFloat(key, _Latency);
	}

	public bool HasSettings()
	{
		string key = LATENCY_KEY + GetAudioOutputName();
		
		return PlayerPrefs.HasKey(key);
	}

	void InvokeAudioSourceChanged()
	{
		m_SignalBus.Fire<AudioSourceChangedSignal>();
	}
}
