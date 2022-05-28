using System;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
	static readonly List<Func<bool>> m_Parameters = new List<Func<bool>>();
	static readonly List<Func<bool>> m_Escape     = new List<Func<bool>>();

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