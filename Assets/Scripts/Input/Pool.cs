using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : MonoBehaviour
{
	readonly Dictionary<int, Queue<T>> m_Pool      = new Dictionary<int, Queue<T>>();
	readonly Dictionary<int, int>      m_Instances = new Dictionary<int, int>();

	Transform m_Root;

	public void Preload(T _Object, int _Count)
	{
		for (int i = 0; i < _Count; i++)
			Populate(_Object);
	}

	public T Instantiate(T _Object, Transform _Transform)
	{
		T instance = Pop(_Object);
		
		if (instance != null)
			instance.transform.SetParent(_Transform, false);
		
		return instance;
	}

	public bool Remove(T _Instance)
	{
		return Push(_Instance);
	}

	bool Push(T _Instance)
	{
		if (_Instance == null)
			return false;
		
		int instanceID = _Instance.GetInstanceID();
		
		if (!m_Instances.TryGetValue(instanceID, out int objectID))
			return false;
		
		if (!m_Pool.ContainsKey(objectID) || m_Pool[objectID] == null)
			m_Pool[objectID] = new Queue<T>();
		
		m_Pool[objectID].Enqueue(_Instance);
		
		m_Instances.Remove(instanceID);
		
		_Instance.transform.SetParent(m_Root, false);
		
		return true;
	}

	T Pop(T _Object)
	{
		if (_Object == null)
			return null;
		
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
		
		if (m_Root == null)
		{
			GameObject rootObject = new GameObject($"Pool[{typeof(T).Name}]");
			rootObject.layer     = LayerMask.NameToLayer("Hidden");
			rootObject.hideFlags = HideFlags.DontSaveInEditor;
			
			m_Root = rootObject.transform;
			
			#if UNITY_EDITOR
			UnityEditor.SceneVisibilityManager.instance.Hide(rootObject, false);
			UnityEditor.SceneVisibilityManager.instance.DisablePicking(rootObject, false);
			#endif
		}
		
		int objectID = _Object.GetInstanceID();
		
		if (!m_Pool.ContainsKey(objectID) || m_Pool[objectID] == null)
			m_Pool[objectID] = new Queue<T>();
		
		T instance = GameObject.Instantiate(_Object, m_Root);
		
		#if UNITY_EDITOR
		UnityEditor.SceneVisibilityManager.instance.Show(instance.gameObject, true);
		UnityEditor.SceneVisibilityManager.instance.EnablePicking(instance.gameObject, true);
		#endif
		
		m_Pool[objectID].Enqueue(instance);
	}
}