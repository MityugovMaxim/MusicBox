using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

public static class AudioUtility
{
	static Action<AudioClip, int, bool> m_PlayClip;
	static Action<AudioClip>            m_StopClip;
	static Action                       m_StopAllClips;
	static Action<AudioClip, int>       m_SetClipSamplePosition;

	public static void PlayClip(AudioClip _AudioClip)
	{
		#if UNITY_EDITOR
		if (m_PlayClip == null)
		{
			Type type = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil");
			
			MethodInfo methodInfo = type.GetMethod(
				"PlayClip",
				BindingFlags.Public | BindingFlags.Static
			);
			
			m_PlayClip = Delegate.CreateDelegate(typeof(Action<AudioClip, int, bool>), methodInfo) as Action<AudioClip, int, bool>;
		}
		
		m_PlayClip?.Invoke(_AudioClip, 0, false);
		#endif
	}

	public static void SetClipSamplePosition(AudioClip _AudioClip, float _Time)
	{
		#if UNITY_EDITOR
		if (m_SetClipSamplePosition == null)
		{
			Type type = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil");
			
			MethodInfo methodInfo = type.GetMethod(
				"SetClipSamplePosition",
				BindingFlags.Public | BindingFlags.Static
			);
			
			m_SetClipSamplePosition = Delegate.CreateDelegate(typeof(Action<AudioClip, int>), methodInfo) as Action<AudioClip, int>;
		}
		
		m_SetClipSamplePosition?.Invoke(
			_AudioClip,
			(int)MathUtility.Remap(_Time, 0, _AudioClip.length, 0, _AudioClip.samples - 1)
		);
		#endif
	}

	public static void StopClip(AudioClip _AudioClip)
	{
		#if UNITY_EDITOR
		if (m_StopClip == null)
		{
			Type type = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil");
			
			MethodInfo methodInfo = type.GetMethod(
				"StopClip",
				BindingFlags.Public | BindingFlags.Static
			);
			
			m_StopClip = Delegate.CreateDelegate(typeof(Action<AudioClip>), methodInfo) as Action<AudioClip>;
		}
		
		m_StopClip?.Invoke(_AudioClip);
		#endif
	}

	public static void StopAllClips()
	{
		#if UNITY_EDITOR
		if (m_StopAllClips == null)
		{
			Type type = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil");
			
			MethodInfo methodInfo = type.GetMethod(
				"StopAllClips",
				BindingFlags.Public | BindingFlags.Static
			);
			
			m_StopAllClips = Delegate.CreateDelegate(typeof(Action), methodInfo) as Action;
		}
		
		m_StopAllClips?.Invoke();
		#endif
	}
}