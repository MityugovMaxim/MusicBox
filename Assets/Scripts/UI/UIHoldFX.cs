using UnityEngine;
using UnityEngine.Scripting;

public class UIHoldFX : UIEntity
{
	[Preserve]
	public class Pool : FXPool<UIHoldFX> { }

	public float Duration => m_Duration;

	[SerializeField] float m_Duration;
}