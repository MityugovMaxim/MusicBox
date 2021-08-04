using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UITapFX : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UITapFX> { }

	public float Duration => m_Duration;

	[SerializeField] float m_Duration;
}