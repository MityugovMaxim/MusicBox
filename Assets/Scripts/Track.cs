using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Track : ScriptableObject, IEnumerable<Clip>
{
	protected Sequencer Sequencer => m_Sequencer;

	Sequencer m_Sequencer;

	public virtual void Initialize(Sequencer _Sequencer)
	{
		m_Sequencer = _Sequencer;
	}

	public abstract void Sample(float _StartTime, float _FinishTime);

	public abstract IEnumerator<Clip> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected T AddReference<T>() where T : Component
	{
		if (Sequencer == null)
			return null;
		
		T reference = Sequencer.gameObject.AddComponent<T>();
		
		reference.hideFlags = HideFlags.HideAndDontSave;
		
		return reference;
	}

	protected T GetReference<T>(string _Reference) where T : Component
	{
		if (Sequencer == null)
			return null;
		
		Transform transform = m_Sequencer.transform.Find(_Reference);
		
		return transform != null ? transform.GetComponent<T>() : null;
	}
}

public class Track<T> : Track where T : Clip
{
	protected List<T> Clips => m_Clips;

	[SerializeField] List<T> m_Clips;

	readonly List<T> m_Buffer = new List<T>();

	public override void Sample(float _StartTime, float _FinishTime)
	{
		m_Buffer.Clear();
		
		FindClips(m_Buffer, _StartTime, _FinishTime);
		
		foreach (T clip in m_Buffer)
			clip.Sample(_FinishTime);
	}

	public void FindClips(List<T> _Clips, float _StartTime, float _FinishTime)
	{
		_Clips.Clear();
		
		int index = FindClip(_StartTime, _FinishTime);
		
		if (index < 0)
			return;
		
		int minIndex = index;
		while (minIndex > 0)
		{
			T clip = m_Clips[minIndex - 1];
			
			if (_StartTime > clip.FinishTime || _FinishTime < clip.StartTime)
				break;
			
			minIndex--;
		}
		
		int maxIndex = index;
		while (maxIndex < m_Clips.Count - 1)
		{
			T clip = m_Clips[maxIndex + 1];
			
			if (_StartTime > clip.FinishTime || _FinishTime < clip.StartTime)
				break;
			
			maxIndex++;
		}
		
		for (int i = minIndex; i <= maxIndex; i++)
			_Clips.Add(m_Clips[i]);
	}

	public override IEnumerator<Clip> GetEnumerator()
	{
		return m_Clips.GetEnumerator();
	}

	int FindClip(float _StartTime, float _FinishTime)
	{
		int i = 0;
		int j = m_Clips.Count - 1;
		while (i <= j)
		{
			int k = (i + j) / 2;
			
			T clip = m_Clips[k];
			
			if (_StartTime > clip.FinishTime)
				i = k + 1;
			else if (_FinishTime < clip.StartTime)
				j = k - 1;
			else
				return k;
		}
		return -1;
	}
}
