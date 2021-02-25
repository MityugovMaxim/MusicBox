using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ClipDrawerAttribute : Attribute
{
	public Type ClipType { get; }

	public ClipDrawerAttribute(Type _ClipType)
	{
		ClipType = _ClipType;
	}
}

public class ClipDrawer
{
	static readonly Dictionary<Type, Type> m_ClipDrawerTypes = new Dictionary<Type, Type>();

	public static ClipDrawer Create(Clip _Clip)
	{
		if (_Clip == null)
			return null;
		
		Type clipDrawerType = GetClipDrawerType(_Clip.GetType());
		
		return Activator.CreateInstance(clipDrawerType, _Clip) as ClipDrawer;
	}

	static Type GetClipDrawerType(Type _ClipType)
	{
		if (m_ClipDrawerTypes.ContainsKey(_ClipType) && m_ClipDrawerTypes[_ClipType] != null)
			return m_ClipDrawerTypes[_ClipType];
		
		Type[] clipDrawerTypes = typeof(ClipDrawer).GetNestedTypes();
		
		foreach (Type clipDrawerType in clipDrawerTypes)
		{
			ClipDrawerAttribute attribute = clipDrawerType.GetCustomAttribute<ClipDrawerAttribute>();
			
			if (attribute.ClipType == _ClipType)
			{
				m_ClipDrawerTypes[_ClipType] = clipDrawerType;
				
				return clipDrawerType;
			}
		}
		
		return typeof(ClipDrawer);
	}

	protected Clip Clip { get; }

	public ClipDrawer(Clip _Clip)
	{
		Clip = _Clip;
	}

	public virtual void Draw(Rect _Rect)
	{
		EditorGUI.DrawRect(_Rect, Color.black);
		
		EditorGUI.DrawRect(
			new Rect(
				_Rect.xMin - 2,
				_Rect.y,
				4,
				_Rect.height
			),
			Color.white
		);
		
		EditorGUI.DrawRect(
			new Rect(
				_Rect.xMax - 2,
				_Rect.y,
				4,
				_Rect.height
			),
			Color.white
		);
	}
}

[ClipDrawer(typeof(MusicClip))]
public class MusicClipDrawer
{
	
}

public class MusicTrack : Track<MusicClip>
{
	static Texture2D CreateWavemap(AudioClip _AudioClip)
	{
		if (_AudioClip == null)
			return null;
		
		float[] data = new float[_AudioClip.samples];
		
		_AudioClip.GetData(data, 0);
		
		int size = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(data.Length)));
		
		Texture2D wavemap = new Texture2D(size, size, TextureFormat.Alpha8, false);
		
		for (int i = 0; i < data.Length; i++)
		{
			int x = i % size;
			int y = i / size;
			
			wavemap.SetPixel(x, y, new Color(0, 0, 0, data[i]));
		}
		
		wavemap.Apply();
		
		return wavemap;
	}
}

public class MusicClip : Clip
{
	protected override void OnEnter(float _Time)
	{
	}

	protected override void OnUpdate(float _Time)
	{
	}

	protected override void OnExit(float _Time)
	{
	}
}

public class SequencerEditor : EditorWindow
{
	[MenuItem("Window/Sequencer")]
	public void Open()
	{
		SequencerEditor window = GetWindow<SequencerEditor>();
		window.minSize = new Vector2(300, 300);
	}

	void OnGUI()
	{
	}
}

[Serializable]
public abstract class Clip
{
	public float StartTime  => m_StartTime;
	public float FinishTime => m_FinishTime;

	[SerializeField] float m_StartTime;
	[SerializeField] float m_FinishTime;

	bool m_Playing;

	public void Sample(float _Time)
	{
		if (_Time >= StartTime && !m_Playing)
		{
			m_Playing = true;
			OnEnter(_Time);
		}
		
		if (m_Playing)
			OnUpdate(_Time);
		
		if (_Time >= FinishTime && m_Playing)
		{
			m_Playing = false;
			OnExit(_Time);
		}
	}

	protected abstract void OnEnter(float _Time);

	protected abstract void OnUpdate(float _Time);

	protected abstract void OnExit(float _Time);
}

public abstract class Track : ScriptableObject, IEnumerable<Clip>
{
	Sequencer m_Sequencer;

	public void Initialize(Sequencer _Sequencer)
	{
		m_Sequencer = _Sequencer;
	}

	public abstract void Sample(float _StartTime, float _FinishTime);

	public abstract IEnumerator<Clip> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected T GetReference<T>(string _Reference) where T : Component
	{
		if (m_Sequencer == null)
			return null;
		
		Transform transform = m_Sequencer.transform.Find(_Reference);
		
		return transform != null ? transform.GetComponent<T>() : null;
	}
}

public class Sequencer : MonoBehaviour
{
	[SerializeField] Track[] m_Tracks;
}

public class Track<T> : Track where T : Clip
{
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

public class RhythmIndicator : MonoBehaviour
{
	[SerializeField] RhythmItem m_RhythmItem;

	readonly Queue<RhythmItem> m_RhythmItems = new Queue<RhythmItem>();

	IEnumerator Start()
	{
		yield return null;
		
		yield return new WaitForSeconds(2);
		
		Play(0.3f);
		
		yield return new WaitForSeconds(0.2f);
		
		Success();
		
		yield return new WaitForSeconds(2);
		
		Play(0.3f);
		
		yield return new WaitForSeconds(0.2f);
		
		Fail();
	}

	public void Play(float _Duration)
	{
		RhythmItem rhythmItem = Instantiate(m_RhythmItem, transform);
		
		rhythmItem.Play(_Duration);
		
		m_RhythmItems.Enqueue(rhythmItem);
	}

	public void Success()
	{
		RhythmItem rhythmItem = m_RhythmItems.Dequeue();
		
		rhythmItem.Success(() => rhythmItem.Remove());
	}

	public void Fail()
	{
		RhythmItem rhythmItem = m_RhythmItems.Dequeue();
		
		rhythmItem.Fail(() => rhythmItem.Remove());
	}
}