using UnityEngine;

public class HoldClip : Clip
{
	public HoldCurve Curve => m_Curve;

	[SerializeField] HoldCurve m_Curve;

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time) { }
}