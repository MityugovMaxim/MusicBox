using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIHoldFX : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIHoldFX> { }

	public float Duration => m_Duration;

	[SerializeField] float m_Duration;
}