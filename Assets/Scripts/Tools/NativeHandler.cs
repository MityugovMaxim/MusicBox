using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NativeHandler : MonoBehaviour
{
	static readonly List<Func<bool>> m_Parameters = new List<Func<bool>>();
	static readonly List<Func<bool>> m_Escape     = new List<Func<bool>>();

	[Inject] AudioManager m_AudioManager;

	public static void RegisterParameters(Func<bool> _Action)
	{
		m_Parameters.Add(_Action);
	}

	public static void UnregisterParameters(Func<bool> _Action)
	{
		m_Parameters.Remove(_Action);
	}

	public static void RegisterEscape(Func<bool> _Action)
	{
		m_Escape.Add(_Action);
	}

	public static void UnregisterEscape(Func<bool> _Action)
	{
		m_Escape.Remove(_Action);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Menu))
			InvokeAction(m_Parameters);
		
		if (Input.GetKeyDown(KeyCode.Escape))
			InvokeAction(m_Escape);
	}

	void OnApplicationFocus(bool _Focus)
	{
		if (m_AudioManager != null && _Focus)
			m_AudioManager.SetAudioActive(true);
	}

	static void InvokeAction(IReadOnlyList<Func<bool>> _Actions)
	{
		for (int i = _Actions.Count - 1; i >= 0; i--)
		{
			Func<bool> action = _Actions[i];
			
			if (action != null && action.Invoke())
				return;
		}
	}
}
