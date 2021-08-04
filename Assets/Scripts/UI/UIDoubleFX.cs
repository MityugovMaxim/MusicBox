using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIDoubleFX : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIDoubleFX> { }

	public float Duration => m_Duration;

	[SerializeField] float m_Duration;
}