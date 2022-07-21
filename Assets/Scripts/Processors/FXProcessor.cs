using UnityEngine;
using Zenject;

public class FXProcessor : UIOrder
{
	[SerializeField] UIEntity        m_Container;
	[SerializeField] UIFXHighlight[] m_Highlights;
	[SerializeField] UIIndicatorFX[] m_SingleFXs;
	[SerializeField] UIIndicatorFX   m_DoubleFX;
	[SerializeField] UIFXHighlight   m_Flash;
	[SerializeField] UIFXHighlight   m_Dim;

	[Inject] ScoreManager m_ScoreManager;

	public void Setup(float _Ratio)
	{
		Vector2 anchorMin = m_Container.RectTransform.anchorMin;
		Vector2 anchorMax = m_Container.RectTransform.anchorMax;
		
		anchorMin.y = 1.0f - _Ratio;
		anchorMax.y = 1.0f - _Ratio;
		
		m_Container.RectTransform.anchorMin = anchorMin;
		m_Container.RectTransform.anchorMax = anchorMax;
	}

	public void TapFX(Rect _Rect, float _Progress)
	{
		int index = GetIndex(_Rect.center);
		
		ScoreGrade grade = m_ScoreManager.GetGrade(ScoreType.Tap, _Progress);
		
		Highlight(index);
		
		Single(index, grade);
	}

	public void DoubleFX(Rect _Rect, float _Progress)
	{
		ScoreGrade grade = m_ScoreManager.GetGrade(ScoreType.Double, _Progress);
		
		Flash();
		
		Double(grade);
	}

	public void HoldFX(Rect _Rect, float _Progress)
	{
		int index = GetIndex(_Rect.center);
		
		ScoreGrade grade = m_ScoreManager.GetGrade(ScoreType.Hold, _Progress);
		
		Highlight(index);
		
		Single(index, grade);
	}

	public void Fail()
	{
		Dim();
	}

	void Flash()
	{
		m_Flash.Play();
	}

	void Dim()
	{
		m_Dim.Play();
	}

	int GetIndex(Vector2 _Position)
	{
		Vector2 position = m_Container.GetLocalPoint(_Position);
		
		Rect rect = m_Container.GetLocalRect();
		
		float phase = MathUtility.Remap(position.x, rect.xMin, rect.xMax, 0, 4);
		
		return Mathf.Clamp((int)phase, 0, 3);
	}

	void Highlight(int _Index)
	{
		UIFXHighlight highlight = m_Highlights[_Index];
		
		highlight.Play();
	}

	void Single(int _Index, ScoreGrade _Grade)
	{
		UIIndicatorFX fx = m_SingleFXs[_Index];
		
		fx.Play(_Grade);
	}

	void Double(ScoreGrade _Grade)
	{
		m_DoubleFX.Play(_Grade);
	}
}