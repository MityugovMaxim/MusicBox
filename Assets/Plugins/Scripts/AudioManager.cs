using System;
using UnityEngine;
using Zenject;

public enum AudioOutputType
{
	BuiltIn    = 0,
	Headphones = 1,
	Bluetooth  = 2,
	Unknown    = 3,
}

public abstract class AudioManager : IInitializable, IDisposable
{
	public event Action OnSourceChange;

	protected delegate void RemoteCommandHandler();

	const string LATENCY_KEY = "LATENCY";

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
		string key = GetLatencyKey();
		
		return PlayerPrefs.GetFloat(key, 0);
	}

	public void SetLatency(float _Latency)
	{
		string key = GetLatencyKey();
		
		PlayerPrefs.SetFloat(key, _Latency);
	}

	public bool HasSettings()
	{
		string key = GetLatencyKey();
		
		return PlayerPrefs.HasKey(key);
	}

	string GetLatencyKey() => $"{LATENCY_KEY}_{GetAudioOutputID()}";

	void InvokeAudioSourceChanged() => OnSourceChange?.Invoke();
}
