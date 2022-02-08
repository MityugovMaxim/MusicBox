using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Menu Registry", menuName = "Registry/Menu Registry")]
public class MenuRegistry : ScriptableObjectInstaller, IEnumerable<MenuInfo>
{
	[SerializeField] List<MenuInfo> m_Registry = new List<MenuInfo>();

	public override void InstallBindings()
	{
		Container.Bind<MenuInfo[]>().FromInstance(m_Registry.ToArray()).AsSingle();
	}

	public bool Contains(ScriptableObject _Object)
	{
		return _Object is MenuInfo entry && m_Registry.Contains(entry);
	}

	public IEnumerator<MenuInfo> GetEnumerator()
	{
		return m_Registry.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}