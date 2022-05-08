using UnityEngine;

public class UIRectsSpectrum : UISpectrum
{
	[SerializeField] SpriteRenderer[] m_Rects;
	[SerializeField] float            m_Distance;
	[SerializeField] float            m_MinSize;
	[SerializeField] float            m_MaxSize;

	public override void Reposition()
	{
		float step = 360.0f / (m_Rects.Length - 1);
		for (int i = 0; i < m_Rects.Length; i++)
		{
			m_Rects[i].transform.localRotation = Quaternion.Euler(0, 0, step * i);
			m_Rects[i].transform.localPosition = m_Rects[i].transform.up * m_Distance;
		}
	}

	public override void Sample(float[] _Buffer)
	{
		for (int i = 0; i < m_Rects.Length; i++)
		{
			Vector2 size = m_Rects[i].size;
			size.y          = Mathf.Lerp(m_MinSize, m_MaxSize, _Buffer[i]);
			m_Rects[i].size = size;
		}
	}
}