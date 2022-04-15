using System;
using System.Runtime.InteropServices;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AudioSourceChangedSignal { }

#if UNITY_EDITOR
[Preserve]
public class EditorAudioManager : AudioManager
{
	protected override void Load(Action _AudioSourceChanged)
	{
		Log.Info(this, "Load completed.");
	}

	protected override void Unload()
	{
		Log.Info(this, "Unload completed.");
	}

	public override void SetAudioActive(bool _Value)
	{
		if (_Value)
			Log.Info(this, "Audio enabled");
		else
			Log.Info(this, "Audio disabled");
	}

	public override string GetAudioOutputName()
	{
		return "Speaker";
	}

	public override AudioOutputType GetAudioOutputType()
	{
		return AudioOutputType.BuiltIn;
	}

	public override string GetAudioOutputID()
	{
		return "speaker_id";
	}
}
#endif

#if UNITY_ANDROID
public class AndroidAudioManager : AudioManager
{
	class AudioSourceChanged : AndroidJavaProxy
	{
		readonly Action m_Action;

		public AudioSourceChanged(Action _Action) : base(HANDLER_NAME)
		{
			m_Action = _Action;
		}

		public void Invoke()
		{
			m_Action?.Invoke();
		}
	}

	const string CLASS_NAME   = "com.audiobox.audiocontroller.AudioController";
	const string HANDLER_NAME = "com.audiobox.audiocontroller.CommandHandler";

	AndroidJavaObject m_AudioController;
	AndroidJavaProxy  m_AudioSourceChanged;

	protected override void Load(Action _AudioSourceChanged)
	{
		m_AudioController = new AndroidJavaObject(CLASS_NAME);
		m_AudioSourceChanged = new  AudioSourceChanged(_AudioSourceChanged);
		
		m_AudioController.Call("Register", m_AudioSourceChanged);
	}

	protected override void Unload()
	{
		m_AudioController.Call("Unregister");
		m_AudioController.Dispose();
	}

	public override void SetAudioActive(bool _Value) { }

	public override string GetAudioOutputName()
	{
		return m_AudioController.Call<string>("GetAudioOutputName");
	}

	public override AudioOutputType GetAudioOutputType()
	{
		return (AudioOutputType)m_AudioController.Call<int>("GetAudioOutputType");
	}

	public override string GetAudioOutputID()
	{
		return m_AudioController.Call<string>("GetAudioOutputID");
	}
}
#endif

#if UNITY_IOS
[Preserve]
public class iOSAudioManager : AudioManager
{
	[DllImport("__Internal")]
	static extern void AudioManager_Register(RemoteCommandHandler _AudioSourceChangedHandler);

	[DllImport("_Internal")]
	static extern void AudioManager_Unregister();

	[DllImport("__Internal")]
	static extern void AudioManager_EnableAudio();

	[DllImport("__Internal")]
	static extern void AudioManager_DisableAudio();

	[DllImport("__Internal")]
	static extern string AudioManager_GetOutputName();

	[DllImport("__Internal")]
	static extern string AudioManager_GetOutputUID();

	[DllImport("__Internal")]
	static extern int AudioManager_GetOutputType();

	static Action m_AudioSourceChanged;

	protected override void Load(Action _AudioSourceChanged)
	{
		m_AudioSourceChanged = _AudioSourceChanged;
		
		AudioManager_Register(AudioSourceChangedHandler);
	}

	protected override void Unload()
	{
		AudioManager_Unregister();
	}

	public override void SetAudioActive(bool _Value)
	{
		if (_Value)
			AudioManager_EnableAudio();
		else
			AudioManager_DisableAudio();
	}

	public override string GetAudioOutputName()
	{
		return AudioManager_GetOutputName();
	}

	public override string GetAudioOutputID()
	{
		return AudioManager_GetOutputUID();
	}

	public override AudioOutputType GetAudioOutputType()
	{
		return (AudioOutputType)AudioManager_GetOutputType();
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void AudioSourceChangedHandler()
	{
		m_AudioSourceChanged?.Invoke();
	}
}
#endif

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

	void InvokeAudioSourceChanged()
	{
		m_SignalBus.Fire<AudioSourceChangedSignal>();
	}
}
