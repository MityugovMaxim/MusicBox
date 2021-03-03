using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public partial class Track
{
	public virtual float MinHeight => 60;
	public virtual float MaxHeight => 200;

	[SerializeField] float m_Height;
}
#endif

public abstract partial class Track : ScriptableObject, IEnumerable<Clip>
{
	protected Sequencer Sequencer { get; private set; }

	public virtual void Initialize(Sequencer _Sequencer)
	{
		Sequencer = _Sequencer;
	}

	public abstract void Sample(float _MinTime, float _MaxTime);

	public abstract void Stop(float _Time);

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
		
		Transform transform = Sequencer.transform.Find(_Reference);
		
		return transform != null ? transform.GetComponent<T>() : null;
	}
}

public class Track<T> : Track where T : Clip
{
	protected List<T> Clips => m_Clips;

	[SerializeField] List<T> m_Clips;

	readonly List<T> m_Buffer = new List<T>();

	public override void Sample(float _MinTime, float _MaxTime)
	{
		m_Buffer.Clear();
		
		FindClips(m_Buffer, _MinTime, _MaxTime);
		
		foreach (T clip in m_Buffer)
			clip.Sample(_MaxTime);
	}

	public override void Stop(float _Time)
	{
		m_Buffer.Clear();
		
		FindClips(m_Buffer, _Time, _Time);
		
		foreach (T clip in m_Buffer)
			clip.Stop(_Time);
	}

	public void FindClips(List<T> _Clips, float _MinTime, float _MaxTime)
	{
		_Clips.Clear();
		
		int index = FindClip(_MinTime, _MaxTime);
		
		if (index < 0)
			return;
		
		int minIndex = index;
		while (minIndex > 0)
		{
			T clip = m_Clips[minIndex - 1];
			
			if (_MinTime > clip.MaxTime || _MaxTime < clip.MinTime)
				break;
			
			minIndex--;
		}
		
		int maxIndex = index;
		while (maxIndex < m_Clips.Count - 1)
		{
			T clip = m_Clips[maxIndex + 1];
			
			if (_MinTime > clip.MaxTime || _MaxTime < clip.MinTime)
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

	int FindClip(float _MinTime, float _MaxTime)
	{
		int i = 0;
		int j = m_Clips.Count - 1;
		while (i <= j)
		{
			int k = (i + j) / 2;
			
			T clip = m_Clips[k];
			
			if (_MinTime > clip.MaxTime)
				i = k + 1;
			else if (_MaxTime < clip.MinTime)
				j = k - 1;
			else
				return k;
		}
		return -1;
	}
}
