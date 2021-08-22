using UnityEngine;
using UnityEngine.Scripting;

public class UITapFX : UIEntity
{
	[Preserve]
	public class Pool : FXPool<UITapFX> { }

	public float Duration => m_Duration;

	[SerializeField] float m_Duration;
}