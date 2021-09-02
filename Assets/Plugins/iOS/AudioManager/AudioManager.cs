// ReSharper disable RedundantUsingDirective

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Zenject;

public class AudioPlaySignal { }
public class AudioPauseSignal { }
public class AudioNextTrackSignal { }
public class AudioPreviousTrackSignal { }
public class AudioSourceChangedSignal { }

public class AudioManager : IInitializable
{
	delegate void RemoteCommandHandler();

	#if UNITY_IOS && !UNITY_EDITOR
	[DllImport("__Internal")]
	static extern float GetInputLatency();

	[DllImport("__Internal")]
	static extern float GetOutputLatency();

	[DllImport("__Internal")]
	static extern void RegisterRemoteCommands(
		RemoteCommandHandler _PlayHandler,
		RemoteCommandHandler _PauseHandler,
		RemoteCommandHandler _NextTrackHandler,
		RemoteCommandHandler _PreviousTrackHandler,
		RemoteCommandHandler _SourceChangedHandler
	);

	[DllImport("_Internal")]
	static extern void UnregisterRemoteCommands();

	[DllImport("__Internal")]
	static extern void EnableAudio();

	[DllImport("__Internal")]
	static extern void DisableAudio();

	[DllImport("__Internal")]
	static extern void GetOutputName();

	[DllImport("__Internal")]
	static extern void GetOutputUID();

	[DllImport("__Internal")]
	static extern void IsOutputWireless();
	#endif

	public static float Latency { get; private set; }

	static SignalBus m_SignalBus;

	[Inject]
	public AudioManager(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
		
		Latency = GetLatency();
	}

	void IInitializable.Initialize()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		RegisterRemoteCommands(
			PlayHandler,
			PauseHandler,
			NextTrackHandler,
			PreviousTrackHandler,
			SourceChangedHandler
		);
		#endif
	}

	public static void SetAudioActive(bool _Value)
	{
		#if UNITY_IOS && !UNITY_EDITOR
		if (_Value)
			EnableAudio();
		else
			DisableAudio();
		#endif
	}

	public static string GetAudioOutputName()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		return GetOutputName();
		#else
		return "Default speakers";
		#endif
	}

	public static string GetAudioOutputUID()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		return GetOutputUID();
		#else
		return string.Empty;
		#endif
	}

	public static bool IsAudioOutputWireless()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		return IsOutputWireless();
		#else
		return false;
		#endif
	}

	static float GetLatency()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		float latency = GetOutputLatency();
		if (latency > 0)
			Debug.LogFormat("[AudioManager] Detected {0}ms latency.", latency);
		return latency;
		#else
		return 0;
		#endif
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void PlayHandler()
	{
		m_SignalBus.Fire(new AudioPlaySignal());
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void PauseHandler()
	{
		m_SignalBus.Fire(new AudioPauseSignal());
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void NextTrackHandler()
	{
		m_SignalBus.Fire(new AudioNextTrackSignal());
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void PreviousTrackHandler()
	{
		m_SignalBus.Fire(new AudioPreviousTrackSignal());
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void SourceChangedHandler()
	{
		Latency = GetLatency();
		
		m_SignalBus.Fire(new AudioSourceChangedSignal());
	}
}
