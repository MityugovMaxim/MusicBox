using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UILevelElement : UIEntity
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

	[SerializeField] GameObject      m_Content;
	[SerializeField] UILevel         m_Level;
	[SerializeField] UILevelProgress m_Progress;
	[SerializeField] CanvasGroup     m_ProgressGroup;

	[Header("Alpha")]
	[SerializeField] float m_SourceAlpha = 0;
	[SerializeField] float m_TargetAlpha = 1;

	[Header("Position")]
	[SerializeField] float m_SourcePosition;
	[SerializeField] float m_TargetPosition;

	[Header("Scale")]
	[SerializeField] float m_SourceScale = 1;
	[SerializeField] float m_TargetScale = 1;

	[Header("Progress")]
	[SerializeField] float m_SourceProgressAlpha = 0;
	[SerializeField] float m_TargetProgressAlpha = 1;

	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;

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
		
		if (m_CanvasGroup == null)
			m_CanvasGroup = GetComponent<CanvasGroup>();
		
		ProcessPhase();
	}
	#endif

	public void Setup(int _Level, int _Discs)
	{
		int minLevel = m_ProgressProcessor.GetMinLevel();
		int maxLevel = m_ProgressProcessor.GetMaxLevel();
		
		m_Content.SetActive(_Level >= minLevel && _Level <= maxLevel);
		
		m_Level.Level = _Level;
		
		m_Progress.Setup(
			_Discs,
			m_ProgressProcessor.GetDiscs(_Level),
			m_ProgressProcessor.GetDiscs(_Level + 1)
		);
	}

	public Task IncrementAsync() => m_Progress.IncrementAsync();

	void ProcessPhase()
	{
		if (m_CanvasGroup != null)
			m_CanvasGroup.alpha = Mathf.Lerp(m_SourceAlpha, m_TargetAlpha, Phase);
		
		Vector2 position = RectTransform.anchoredPosition;
		position.x                     = Mathf.Lerp(m_SourcePosition, m_TargetPosition, Phase);
		RectTransform.anchoredPosition = position;
		
		float scale = Mathf.Lerp(m_SourceScale, m_TargetScale, Phase);
		RectTransform.localScale = new Vector3(scale, scale, 1);
		
		m_ProgressGroup.alpha = Mathf.Lerp(m_SourceProgressAlpha, m_TargetProgressAlpha, Phase);
	}
}