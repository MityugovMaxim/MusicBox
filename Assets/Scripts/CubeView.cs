using System.Collections;
using UnityEngine;

public class CubeView : BeatView
{
	[SerializeField] float m_Amplitude;

	IEnumerator m_RotateRoutine;

	public override void Play(float _Duration)
	{
		base.Play(_Duration);
		
		if (m_RotateRoutine != null)
			StopCoroutine(m_RotateRoutine);
		
		m_RotateRoutine = RotateRoutine(_Duration);
		
		StartCoroutine(m_RotateRoutine);
	}

	IEnumerator RotateRoutine(float _Duration)
	{
		Quaternion source = Quaternion.identity;
		Quaternion target = Quaternion.Euler(
			m_Amplitude * (Random.value * 2 - 1),
			m_Amplitude * (Random.value * 2 - 1),
			m_Amplitude * (Random.value * 2 - 1)
		);
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = time / _Duration;
			
			transform.rotation = Quaternion.Lerp(source, target, Mathf.PingPong(phase * 2, 1));
		}
		
		transform.rotation = source;
	}
}