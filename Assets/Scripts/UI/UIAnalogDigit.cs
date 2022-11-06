using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIAnalogDigit : UIEntity
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

	[SerializeField] GameObject m_Content;
	[SerializeField] Graphic    m_Background;
	[SerializeField] Image      m_Center;
	[SerializeField] Image      m_Upper;
	[SerializeField] Image      m_Lower;
	[SerializeField] Image      m_Analog;
	[SerializeField] float      m_Duration;
	[SerializeField] Sprite[]   m_Digits;

	int m_Value;

	CancellationTokenSource m_TokenSource;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	[ContextMenu("Test")]
	public async void Test()
	{
		await SetValueAsync(m_Value + 1);
	}

	public async Task SetValueAsync(int _Value, bool _Instant = false, CancellationToken _Token = default)
	{
		int value = Mathf.Abs(_Value) % 10;
		
		if (m_Value == value)
			return;
		
		if (_Token.IsCancellationRequested || _Instant)
		{
			m_Value = value;
			m_Content.SetActive(false);
			m_Upper.sprite = m_Digits[m_Value];
			m_Lower.sprite = m_Digits[m_Value];
			return;
		}
		
		m_Upper.sprite  = m_Digits[value];
		m_Center.sprite = m_Digits[m_Value];
		
		m_Value = value;
		
		m_Content.SetActive(true);
		
		await UnityTask.Phase(
			_Phase => Phase = _Phase,
			m_Duration,
			EaseFunction.EaseInQuad,
			_Token
		);
		
		Phase = 0;
		
		m_Lower.sprite = m_Digits[value];
		
		m_Content.SetActive(false);
	}

	void ProcessPhase()
	{
		ProcessFill();
		
		ProcessScale();
		
		ProcessColor();
		
		ProcessContent();
	}

	void ProcessFill()
	{
		m_Center.fillOrigin = Phase >= 0.5f ? 0 : 1;
		
		if (Phase >= 0.5f)
			m_Center.sprite = m_Digits[m_Value];
	}

	void ProcessScale()
	{
		Vector3 centerScale = m_Center.rectTransform.localScale;
		centerScale.y                     = Mathf.Lerp(1, 0, Mathf.PingPong(Phase * 2, 1));
		m_Center.rectTransform.localScale = centerScale;
		
		Vector3 analogScale = m_Analog.rectTransform.localScale;
		analogScale.y                     = Mathf.Lerp(1, -1, Phase);
		m_Analog.rectTransform.localScale = analogScale;
	}

	void ProcessColor()
	{
		float multiplier = Phase < 0.5f
			? Mathf.Lerp(1, 0.7f, MathUtility.Remap01(Phase, 0, 0.5f))
			: Mathf.Lerp(1, 1.3f, MathUtility.Remap01(Phase, 1, 0.5f));
		
		Color centerColor = m_Upper.color;
		centerColor.r *= multiplier;
		centerColor.g *= multiplier;
		centerColor.b *= multiplier;
		
		Color analogColor = m_Background.color;
		analogColor.r *= multiplier;
		analogColor.g *= multiplier;
		analogColor.b *= multiplier;
		
		m_Center.color = centerColor;
		m_Analog.color = analogColor;
	}

	void ProcessContent()
	{
		m_Content.SetActive(Phase > float.Epsilon && Phase < 1);
	}
}
