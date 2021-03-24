using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : MonoBehaviour
{
	Transform Root
	{
		get
		{
			if (m_Root == null)
			{
				GameObject rootObject = new GameObject($"Pool[{typeof(T).Name}]");
				rootObject.layer     = LayerMask.NameToLayer("Hidden");
				rootObject.hideFlags = HideFlags.HideAndDontSave;
				
				m_Root = rootObject.transform;
			}
			return m_Root;
		}
	}

	readonly Dictionary<int, Queue<T>> m_Pool      = new Dictionary<int, Queue<T>>();
	readonly Dictionary<int, int>      m_Instances = new Dictionary<int, int>();

	Transform m_Root;

	public Pool() { }

	public Pool(T _Object, int _Count)
	{
		Preload(_Object, _Count);
	}

	public T Instantiate(T _Object, Transform _Transform)
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying)
			return Object.Instantiate(_Object, _Transform, false);
		#endif
		
		T instance = Pop(_Object);
		
		if (instance != null)
			instance.transform.SetParent(_Transform, false);
		
		return instance;
	}

	public bool Remove(T _Instance)
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			Object.DestroyImmediate(_Instance.gameObject, true);
			return true;
		}
		#endif
		
		if (_Instance != null)
			_Instance.transform.SetParent(Root, false);
		
		return Push(_Instance);
	}

	public void Release()
	{
		m_Pool.Clear();
		
		m_Instances.Clear();
		
		if (Root != null)
			Object.DestroyImmediate(Root.gameObject);
		
		Resources.UnloadUnusedAssets();
	}

	void Preload(T _Object, int _Count)
	{
		for (int i = 0; i < _Count; i++)
			Populate(_Object);
	}

	bool Push(T _Instance)
	{
		if (_Instance == null)
		{
			Debug.LogError($"[Pool<{typeof(T).Name}>] Push failed. Instance is null.");
			return false;
		}
		
		int instanceID = _Instance.GetInstanceID();
		
		if (!m_Instances.TryGetValue(instanceID, out int objectID))
		{
			Debug.LogError($"[Pool<{typeof(T).Name}>] Push failed. Instance is not belongs to this pool.");
			return false;
		}
		
		if (!m_Pool.ContainsKey(objectID) || m_Pool[objectID] == null)
			m_Pool[objectID] = new Queue<T>();
		
		m_Pool[objectID].Enqueue(_Instance);
		
		m_Instances.Remove(instanceID);
		
		return true;
	}

	T Pop(T _Object)
	{
		if (_Object == null)
		{
			Debug.LogError($"[Pool<{typeof(T).Name}>] Pop failed. Object is null.");
			return null;
		}
		
		int objectID = _Object.GetInstanceID();
		
		if (!m_Pool.ContainsKey(objectID) || m_Pool[objectID] == null || m_Pool[objectID].Count == 0)
			Populate(_Object);
		
		T instance = m_Pool[objectID].Dequeue();
		
		int instanceID = instance.GetInstanceID();
		
		m_Instances.Add(instanceID, objectID);
		
		return instance;
	}

	void Populate(T _Object)
	{
		if (_Object == null)
			return;
		
		int objectID = _Object.GetInstanceID();
		
		if (!m_Pool.ContainsKey(objectID) || m_Pool[objectID] == null)
			m_Pool[objectID] = new Queue<T>();
		
		T instance = GameObject.Instantiate(_Object, Root, false);
		
		m_Pool[objectID].Enqueue(instance);
	}
}