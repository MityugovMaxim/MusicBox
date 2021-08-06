using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Registry : ScriptableObject
{
	public abstract Type AssetType { get; }

	public abstract string Name { get; }

	public abstract void Add(ScriptableObject _Object);

	public abstract bool Contains(ScriptableObject _Object);

	public static T Load<T>(string _Name) where T : Registry
	{
		return Resources.Load<T>($"Registries/{_Name}");
	}
}

public abstract class Registry<T> : Registry, IEnumerable<T> where T : ScriptableObject
{
	public override Type AssetType => typeof(T);

	public override void Add(ScriptableObject _Object)
	{
		if (_Object is T entry)
			m_Registry.Add(entry);
	}

	public override bool Contains(ScriptableObject _Object)
	{
		return _Object is T entry && m_Registry.Contains(entry);
	}

	public T this[int _Index] => m_Registry[_Index];

	public int Length => m_Registry.Count;

	[SerializeField] List<T> m_Registry;

	public IEnumerator<T> GetEnumerator()
	{
		return m_Registry.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
