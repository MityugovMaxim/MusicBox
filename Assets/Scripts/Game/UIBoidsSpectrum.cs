using UnityEngine;

public class UIBoidsSpectrum : UISpectrum
{
	[SerializeField] Transform[] m_Boids;
	[SerializeField] float       m_MinDistance;
	[SerializeField] float       m_MaxDistance;

	public override void Reposition()
	{
		float step = 360.0f / (m_Boids.Length - 1);
		for (int i = 0; i < m_Boids.Length; i++)
		{
			m_Boids[i].localRotation = Quaternion.Euler(0, 0, step * i);
			m_Boids[i].localPosition = m_Boids[i].up * m_MinDistance;
		}
	}

	public override void Sample(float[] _Amplitude)
	{
		for (int i = 0; i < m_Boids.Length; i++)
			m_Boids[i].localPosition = m_Boids[i].up * Mathf.Lerp(m_MinDistance, m_MaxDistance, _Amplitude[i]);
	}
}