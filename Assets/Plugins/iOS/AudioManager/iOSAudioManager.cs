using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

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