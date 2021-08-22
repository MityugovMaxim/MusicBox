using System.Collections.Generic;
using UnityEngine;

public class MaterialPool
{
	readonly Dictionary<string, Material> m_Pool = new Dictionary<string, Material>();

	public Material Get(string _MaterialID)
	{
		if (string.IsNullOrEmpty(_MaterialID))
			return null;
		
		if (!m_Pool.ContainsKey(_MaterialID))
			return null;
		
		Material material = m_Pool[_MaterialID];
		
		return material == null ? null : material;
	}

	public void Register(string _MaterialID, Material _Material)
	{
		if (string.IsNullOrEmpty(_MaterialID))
			return;
		
		m_Pool[_MaterialID] = _Material;
	}
}