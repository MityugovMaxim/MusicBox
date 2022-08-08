#if UNITY_ANDROID
using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AndroidAudioManager : AudioManager
{
	class AudioSourceChanged : AndroidJavaProxy
	{
		readonly Action m_Action;

		public AudioSourceChanged(Action _Action) : base(HANDLER_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
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
		m_AudioController    = new AndroidJavaObject(CLASS_NAME);
		m_AudioSourceChanged = new  AudioSourceChanged(_AudioSourceChanged);
		
		m_AudioController.Call("Register", m_AudioSourceChanged);
	}

	protected override void Unload()
	{
		m_AudioController.Call("Unregister");
		m_AudioController.Dispose();
	}

	public override void SetAudioActive(bool _Value)
	{
		if (_Value)
		{
			AudioListener.volume = 1;
			AudioListener.pause  = false;
		}
		else
		{
			AudioListener.volume = 0;
			AudioListener.pause  = true;
		}
	}

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