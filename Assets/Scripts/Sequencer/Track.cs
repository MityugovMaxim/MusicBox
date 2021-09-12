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
		get => Expanded ? Mathf.Clamp(m_Height, MinHeight, MaxHeight) : 30;
		set
		{
			if (Expanded)
				m_Height = Mathf.Clamp(value, MinHeight, MaxHeight);
		}
	}

	public bool Expanded
	{
		get => m_Expanded;
		set => m_Expanded = value;
	}

	public string Mnemonic
	{
		get => m_Mnemonic;
		set => m_Mnemonic = value;
	}

	protected virtual float MinHeight => 30;
	protected virtual float MaxHeight => 200;

	[SerializeField]                  string m_Mnemonic;
	[SerializeField, HideInInspector] float  m_Height;
	[SerializeField, HideInInspector] bool   m_Expanded = true;

	public virtual void DropPerform(float _Time, Object[] _Objects) { }

	public abstract void Sort();
}
#endif

public abstract partial class Track : ScriptableObject, IEnumerable<Clip>, IReferenceResolver
{
	public Sequencer Sequencer { get; private set; }

	protected abstract float Offset { get; }

	public virtual void Initialize(Sequencer _Sequencer)
	{
		Sequencer = _Sequencer;
	}

	public virtual void Dispose() { }

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
		
		Initialize(Sequencer);
	}
}
#endif

public partial class Track<T> : Track where T : Clip
{
	protected override float Offset => AudioManager.Latency;

	protected List<T> Clips => m_Clips;

	[SerializeField] List<T> m_Clips = new List<T>();

	readonly List<T> m_Buffer = new List<T>();

	public override void Dispose()
	{
		foreach (Clip clip in Clips)
			clip.Dispose();
	}

	public override void Sample(float _MinTime, float _MaxTime)
	{
		_MinTime -= Offset;
		_MaxTime -= Offset;
		
		float time = _MaxTime;
		
		bool reverse = _MinTime > _MaxTime;
		
		if (reverse)
			(_MinTime, _MaxTime) = (_MaxTime, _MinTime);
		
		m_Buffer.Clear();
		
		FindClips(m_Buffer, _MinTime, _MaxTime);
		
		if (reverse)
		{
			for (int i = m_Buffer.Count - 1; i >= 0; i--)
				m_Buffer[i].Sample(time);
		}
		else
		{
			foreach (T clip in m_Buffer)
				clip.Sample(time);
		}
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
			
			if (_MinTime > clip.MaxTime + clip.MaxOffset || _MaxTime < clip.MinTime + clip.MinOffset)
				break;
			
			minIndex--;
		}
		
		int maxIndex = index;
		while (maxIndex < m_Clips.Count - 1)
		{
			T clip = m_Clips[maxIndex + 1];
			
			if (_MinTime > clip.MaxTime + clip.MaxOffset || _MaxTime < clip.MinTime + clip.MinOffset)
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
			
			if (_MinTime > clip.MaxTime + clip.MaxOffset)
				i = k + 1;
			else if (_MaxTime < clip.MinTime + clip.MinOffset)
				j = k - 1;
			else
				return k;
		}
		return -1;
	}
}
