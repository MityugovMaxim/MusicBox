using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
public partial class Track
{
	public float Height
	{
		get => Mathf.Clamp(m_Height, MinHeight, MaxHeight);
		set => m_Height = Mathf.Clamp(value, MinHeight, MaxHeight);
	}

	protected virtual float MinHeight => 30;
	protected virtual float MaxHeight => 200;

	[SerializeField, HideInInspector] float m_Height;

	public virtual void DragPerform(float _Time, Object[] _Objects)
	{
		StringBuilder debug = new StringBuilder();
		
		debug.Append($"[{(int)_Time / 60:00}:{(int)_Time % 60:00}{_Time - (int)_Time:.000}]");
		debug.Append(" DragAndDrop: ");
		debug.Append(string.Join(",", _Objects.Select(_Object => _Object.ToString())));
		
		Debug.Log(debug.ToString());
	}
}
#endif

public abstract partial class Track : ScriptableObject, IEnumerable<Clip>, IReferenceResolver
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

	public Component GetContext()
	{
		return Sequencer;
	}

	public T GetReference<T>(string _Reference) where T : Component
	{
		if (Sequencer == null || string.IsNullOrEmpty(_Reference))
			return null;
		
		Transform transform = Sequencer.transform.Find(_Reference);
		
		return transform != null ? transform.GetComponent<T>() : null;
	}

	public Component GetReference(Type _Type, string _Reference)
	{
		if (Sequencer == null || string.IsNullOrEmpty(_Reference))
			return null;
		
		Transform transform = Sequencer.transform.Find(_Reference);
		
		return transform != null ? transform.GetComponent(_Type) : null;
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
