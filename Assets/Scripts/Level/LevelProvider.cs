using System;
using UnityEngine;
using Zenject;

public class LevelProvider
{
	public string Title  => m_Title;
	public string Artist => m_Artist;

	public event Action LevelChanged;

	[Inject] Level.Factory m_LevelFactory;

	Level  m_Level;
	string m_Title;
	string m_Artist;

	public void Create(LevelInfo _LevelInfo)
	{
		if (_LevelInfo == null)
		{
			Debug.LogError("[LevelProvider] Create level failed. Level info is null.");
			return;
		}
		
		if (m_Level != null)
		{
			Debug.LogErrorFormat("[LevelProvider] Create level failed. Level instance '{0}' already created.", m_Level.name);
			return;
		}
		
		m_Level  = m_LevelFactory.Create($"{_LevelInfo.ID}/level");
		m_Title  = _LevelInfo.Title;
		m_Artist = _LevelInfo.Artist;
		
		LevelChanged?.Invoke();
	}

	public void Remove()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Remove level failed. Level is null.");
			return;
		}
		
		GameObject.Destroy(m_Level.gameObject);
		
		m_Level  = null;
		m_Artist = null;
		m_Title  = null;
		
		LevelChanged?.Invoke();
	}

	public void Play()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Play level failed. Level is null.");
			return;
		}
		
		m_Level.Play();
	}

	public void Pause()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Pause level failed. Level is null.");
			return;
		}
		
		m_Level.Pause();
	}

	public void Stop()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Stop level failed. Level is null.");
			return;
		}
		
		m_Level.Stop();
	}
}