using System.Collections;
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
	[SerializeField] Image      m_Center;
	[SerializeField] Image      m_Upper;
	[SerializeField] Image      m_Lower;
	[SerializeField] float      m_Duration;
	[SerializeField] Sprite[]   m_Digits;

	int m_Value;

	IEnumerator m_ValueRoutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Phase = 0;
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying || !IsInstanced)
			return;
		
		ProcessPhase();
	}
	#endif

	public void SetValue(int _Value, bool _Instant = false)
	{
		if (m_ValueRoutine != null)
		{
			StopCoroutine(m_ValueRoutine);
			m_ValueRoutine = null;
		}
		
		int value = Mathf.Abs(_Value) % 10;
		
		if (gameObject.activeInHierarchy && !_Instant)
		{
			m_ValueRoutine = ValueRoutine(value);
			
			StartCoroutine(m_ValueRoutine);
		}
		else
		{
			Phase           = 0;
			m_Value         = value;
			m_Upper.sprite  = m_Digits[m_Value];
			m_Lower.sprite  = m_Digits[m_Value];
			m_Center.sprite = m_Digits[m_Value];
			m_Content.SetActive(false);
		}
	}

	void ProcessPhase()
	{
		ProcessFill();
		
		ProcessScale();
		
		ProcessColor();
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
		
		m_Center.color = centerColor;
	}

	IEnumerator ValueRoutine(int _Value)
	{
		int source = m_Value;
		int target = _Value;
		
		Sprite sourceSprite = m_Digits[source];
		Sprite targetSprite = m_Digits[target];
		
		m_Upper.sprite  = targetSprite;
		m_Lower.sprite  = sourceSprite;
		m_Center.sprite = sourceSprite;
		
		Phase = 0;
		
		if (source != target && m_Duration > float.Epsilon)
		{
			m_Content.SetActive(true);
			
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				Phase = EaseFunction.EaseInQuad.Get(time / m_Duration);
				
				if (Phase >= 0.5f)
					m_Center.sprite = targetSprite;
			}
			
			m_Content.SetActive(false);
		}
		
		m_Value = target;
		
		Phase = 0;
		
		m_Upper.sprite  = targetSprite;
		m_Lower.sprite  = targetSprite;
		m_Center.sprite = targetSprite;
	}
}
