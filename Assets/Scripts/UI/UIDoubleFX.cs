using UnityEngine;
using UnityEngine.Scripting;

public class UIDoubleFX : UIEntity
{
	[Preserve]
	public class Pool : FXPool<UIDoubleFX> { }

	public float Duration => m_Duration;

	[SerializeField] float m_Duration;
}