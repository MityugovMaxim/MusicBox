using UnityEngine;

public class RoutineTest : MonoBehaviour, IRoutineClipReceiver
{
	[SerializeField] Transform m_Target;
	[SerializeField] Vector3   m_SourcePosition;
	[SerializeField] Vector3   m_TargetPosition;

	public void StartRoutine(float _Time)
	{
		m_Target.gameObject.SetActive(true);
		m_Target.localPosition = Vector3.Lerp(m_SourcePosition, m_TargetPosition, _Time);
	}

	public void UpdateRoutine(float _Time)
	{
		m_Target.localPosition = Vector3.Lerp(m_SourcePosition, m_TargetPosition, _Time);
	}

	public void FinishRoutine(float _Time)
	{
		m_Target.gameObject.SetActive(false);
		m_Target.localPosition = Vector3.Lerp(m_SourcePosition, m_TargetPosition, _Time);
	}
}