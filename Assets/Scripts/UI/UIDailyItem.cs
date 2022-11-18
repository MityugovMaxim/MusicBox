using UnityEngine;
using Zenject;

public class UIDailyItem : UIEntity
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
	[SerializeField] GameObject  m_Ads;
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] UIFlare     m_Flare;
	[SerializeField] CanvasGroup m_CanvasGroup;

	[Header("Alpha")]
	[SerializeField] float m_SourceAlpha = 0;
	[SerializeField] float m_TargetAlpha = 1;

	[Header("Position")]
	[SerializeField] float m_SourcePosition = 0;
	[SerializeField] float m_TargetPosition = 0;

	[Header("Scale")]
	[SerializeField] float m_SourceScale = 1;
	[SerializeField] float m_TargetScale = 1;

	[Inject] DailyCollection m_DailyCollection;

	string m_DailyID;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessPhase();
	}
	#endif

	public void Setup(string _DailyID)
	{
		m_DailyID = _DailyID;
		
		m_Content.SetActive(!string.IsNullOrEmpty(m_DailyID));
		m_Ads.SetActive(m_DailyCollection.GetAds(m_DailyID));
		
		m_Coins.Value = m_DailyCollection.GetCoins(m_DailyID);
	}

	public void Collect()
	{
		m_Flare.Play();
	}

	void ProcessPhase()
	{
		if (m_CanvasGroup != null)
			m_CanvasGroup.alpha = Mathf.LerpUnclamped(m_SourceAlpha, m_TargetAlpha, Phase);
		
		Vector2 position = RectTransform.anchoredPosition;
		position.x                     = Mathf.LerpUnclamped(m_SourcePosition, m_TargetPosition, Phase);
		RectTransform.anchoredPosition = position;
		
		float scale = Mathf.LerpUnclamped(m_SourceScale, m_TargetScale, Phase);
		RectTransform.localScale = new Vector3(scale, scale, 1);
	}
}