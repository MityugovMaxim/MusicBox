using UnityEngine;

public class UIRectsSpectrum : UISpectrum
{
	[SerializeField] SpriteRenderer[] m_Rects;
	[SerializeField] RectTransform[]  m_Overlay;
	[SerializeField] float            m_Distance;
	[SerializeField] float            m_MinSize;
	[SerializeField] float            m_MaxSize;
	[SerializeField] float            m_MinHeight;
	[SerializeField] float            m_MaxHeight;

	public override void Reposition()
	{
		float step = 360.0f / (m_Rects.Length - 1);
		for (int i = 0; i < m_Rects.Length; i++)
		{
			m_Rects[i].transform.localRotation = Quaternion.Euler(0, 0, step * i);
			m_Rects[i].transform.localPosition = m_Rects[i].transform.up * m_Distance;
		}
	}

	public override void Sample(float[] _Amplitude)
	{
		for (int i = 0; i < m_Rects.Length; i++)
		{
			Vector2 size = m_Rects[i].size;
			size.y          = Mathf.Lerp(m_MinSize, m_MaxSize, _Amplitude[i]);
			m_Rects[i].size = size;
		}
		
		for (int i = 0; i < m_Overlay.Length; i++)
		{
			Vector2 size = m_Overlay[i].sizeDelta;
			size.y                 = Mathf.Lerp(m_MinHeight, m_MaxHeight, _Amplitude[i]);
			m_Overlay[i].sizeDelta = size;
		}
	}
}