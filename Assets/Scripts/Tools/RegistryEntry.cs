using UnityEngine;

public class RegistryEntry : ScriptableObject
{
	public bool Active => m_Active;

	[SerializeField, HideInInspector] bool m_Active = true;
}