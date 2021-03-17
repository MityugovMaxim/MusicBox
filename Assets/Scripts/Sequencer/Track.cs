using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public virtual void DropPerform(float _Time, Object[] _Objects) { }

	public abstract void Sort();
}
#endif

public abstract partial class Track : ScriptableObject, IEnumerable<Clip>, IReferenceResolver
{
	public Sequencer Sequencer { get; private set; }

	public virtual void Initialize(Sequencer _Sequencer)
	{
		Sequencer = _Sequencer;
	}

	public abstract void Sample(float _MinTime, float _MaxTime);

	public abstract IEnumerator<Clip> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected T AddReference<T>() where T : Component
	{
		if (Sequencer == null)
			return null;
		
		T reference = Sequencer.GetComponent<T>();
		
		if (reference == null)
			reference = Sequencer.gameObject.AddComponent<T>();
		
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

	public T[] GetReferences<T>(string _Reference)
	{
		if (Sequencer == null || string.IsNullOrEmpty(_Reference))
			return null;
		
		Transform transform = Sequencer.transform.Find(_Reference);
		
		return transform.GetComponents<Component>().OfType<T>().ToArray();
	}

	public GameObject GetReference(string _Reference)
	{
		if (Sequencer == null || string.IsNullOrEmpty(_Reference))
			return null;
		
		Transform transform = Sequencer.transform.Find(_Reference);
		
		return transform != null ? transform.gameObject : null;
	}

	public Object GetReference(Type _Type, string _Reference)
	{
		if (Sequencer == null || string.IsNullOrEmpty(_Reference))
			return null;
		
		Transform transform = Sequencer.transform.Find(_Reference);
		
		if (transform == null)
			return null;
		
		if (_Type == typeof(GameObject))
			return transform.gameObject;
		
		return transform.GetComponent(_Type);
	}
}

#if UNITY_EDITOR
public partial class Track<T>
{
	public override void Sort()
	{
		Clips.Sort((_A, _B) => _A.MinTime.CompareTo(_B.MinTime));
	}
}
#endif

public partial class Track<T> : Track where T : Clip
{
	protected List<T> Clips => m_Clips;

	[SerializeField] float   m_Offset;
	[SerializeField] List<T> m_Clips;

	readonly List<T> m_Buffer = new List<T>();

	public override void Sample(float _MinTime, float _MaxTime)
	{
		_MinTime += m_Offset;
		_MaxTime += m_Offset;
		
		float time = _MaxTime;
		
		if (_MinTime > _MaxTime)
		{
			float buffer = _MinTime;
			_MinTime = _MaxTime;
			_MaxTime = buffer;
		}
		
		m_Buffer.Clear();
		
		FindClips(m_Buffer, _MinTime, _MaxTime);
		
		foreach (T clip in m_Buffer)
			clip.Sample(time);
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
