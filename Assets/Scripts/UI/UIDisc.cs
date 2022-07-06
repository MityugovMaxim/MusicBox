using UnityEngine;
using UnityEngine.UI;

public class UIDisc : UIEntity
{
	public ScoreRank Rank
	{
		get => m_Rank;
		set
		{
			if (m_Rank == value)
				return;
			
			m_Rank = value;
			
			ProcessRank();
		}
	}

	public Color Color
	{
		get => m_Color;
		set
		{
			if (m_Color == value)
				return;
			
			m_Color = value;
			
			ProcessColor();
		}
	}

	public float Alpha
	{
		get => Color.a;
		set
		{
			Color color = Color;
			color.a = value;
			Color   = color;
		}
	}

	[SerializeField] ScoreRank m_Rank;
	[SerializeField] Color     m_Color;
	[SerializeField] Image     m_Graphic;
	[SerializeField] Sprite    m_Bronze;
	[SerializeField] Sprite    m_Silver;
	[SerializeField] Sprite    m_Gold;
	[SerializeField] Sprite    m_Platinum;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessRank();
		
		ProcessColor();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessRank();
		
		ProcessColor();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessRank();
		
		ProcessColor();
	}
	#endif

	void ProcessRank()
	{
		m_Graphic.enabled = Rank != ScoreRank.None;
		switch (Rank)
		{
			case ScoreRank.Bronze:
				m_Graphic.sprite = m_Bronze;
				break;
			case ScoreRank.Silver:
				m_Graphic.sprite = m_Silver;
				break;
			case ScoreRank.Gold:
				m_Graphic.sprite = m_Gold;
				break;
			case ScoreRank.Platinum:
				m_Graphic.sprite = m_Platinum;
				break;
		}
	}

	void ProcessColor()
	{
		m_Graphic.color = Color;
	}
}