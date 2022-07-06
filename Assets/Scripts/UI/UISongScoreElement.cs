using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasGroup))]
public class UISongScoreElement : UIEntity
{
	public float Phase
	{
		get => m_Phase;
		set
		{
			if (Mathf.Approximately(m_Phase, value))
				return;
			
			m_Phase = value;
			
			ProcessPhase();
		}
	}

	[SerializeField, Range(0, 1)] float m_Phase;

	[SerializeField] GameObject  m_Content;
	[SerializeField] GameObject  m_Record;
	[SerializeField] TMP_Text    m_Position;
	[SerializeField] UIUnitLabel m_Score;
	[SerializeField] UIDisc      m_Disc;

	[Header("Alpha")]
	[SerializeField] float m_SourceAlpha = 0;
	[SerializeField] float m_TargetAlpha = 1;

	[Header("Scale")]
	[SerializeField] float m_SourceScale = 0.5f;
	[SerializeField] float m_TargetScale = 1;

	[Header("Position")]
	[SerializeField] float m_SourcePosition;
	[SerializeField] float m_TargetPosition;

	CanvasGroup m_CanvasGroup;

	protected override void Awake()
	{
		base.Awake();
		
		m_CanvasGroup = GetComponent<CanvasGroup>();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	public void Setup(int _Position, long _Score, ScoreRank _Rank, bool _Record)
	{
		int position = 100 - _Position + 1;
		
		m_Content.SetActive(position >= 1 && position <= 100);
		m_Record.SetActive(_Record);
		
		m_Position.text = position.ToString();
		m_Score.Value   = _Score;
		m_Disc.Rank     = _Rank;
	}

	void ProcessPhase()
	{
		if (m_CanvasGroup != null)
			m_CanvasGroup.alpha = Mathf.Lerp(m_SourceAlpha, m_TargetAlpha, Phase);
		
		Vector2 position = RectTransform.anchoredPosition;
		position.y = Mathf.Lerp(m_SourcePosition, m_TargetPosition, Phase);
		RectTransform.anchoredPosition = position;
		
		float scale = Mathf.Lerp(m_SourceScale, m_TargetScale, Phase);
		RectTransform.localScale = new Vector3(scale, scale, 1);
	}
}