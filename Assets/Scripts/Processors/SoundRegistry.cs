using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Sound Registry", menuName = "Registry/Sound Registry")]
public class SoundRegistry : ScriptableObjectInstaller, IEnumerable<SoundInfo>
{
	[SerializeField] List<SoundInfo> m_Registry = new List<SoundInfo>();

	public override void InstallBindings()
	{
		Container.Bind<SoundInfo[]>().FromInstance(m_Registry.ToArray()).AsSingle();
	}

	public bool Contains(ScriptableObject _Object)
	{
		return _Object is SoundInfo entry && m_Registry.Contains(entry);
	}

	public IEnumerator<SoundInfo> GetEnumerator()
	{
		return m_Registry.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}