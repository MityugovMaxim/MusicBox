#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

[Preserve]
public class iOSAudioManager : AudioManager
{
	[DllImport("__Internal")]
	static extern void AudioController_Register(RemoteCommandHandler _AudioSourceChangedHandler);

	[DllImport("_Internal")]
	static extern void AudioController_Unregister();

	[DllImport("__Internal")]
	static extern void AudioController_EnableAudio();

	[DllImport("__Internal")]
	static extern void AudioController_DisableAudio();

	[DllImport("__Internal")]
	static extern string AudioController_GetOutputName();

	[DllImport("__Internal")]
	static extern string AudioController_GetOutputID();

	[DllImport("__Internal")]
	static extern int AudioController_GetOutputType();

	static Action m_AudioSourceChanged;

	protected override void Load(Action _AudioSourceChanged)
	{
		m_AudioSourceChanged = _AudioSourceChanged;
		
		AudioController_Register(AudioSourceChangedHandler);
	}

	protected override void Unload()
	{
		AudioController_Unregister();
	}

	public override void SetAudioActive(bool _Value)
	{
		if (_Value)
			AudioController_EnableAudio();
		else
			AudioController_DisableAudio();
	}

	public override string GetAudioOutputName()
	{
		return AudioController_GetOutputName();
	}

	public override string GetAudioOutputID()
	{
		return AudioController_GetOutputID();
	}

	public override AudioOutputType GetAudioOutputType()
	{
		return (AudioOutputType)AudioController_GetOutputType();
	}

	[AOT.MonoPInvokeCallback(typeof(RemoteCommandHandler))]
	static void AudioSourceChangedHandler()
	{
		m_AudioSourceChanged?.Invoke();
	}
}
#endif