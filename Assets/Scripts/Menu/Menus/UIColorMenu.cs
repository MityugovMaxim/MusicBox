using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ColorMenu)]
public class UIColorMenu : UIMenu
{
	[SerializeField] UIColorSlider m_SliderRed;
	[SerializeField] UIColorSlider m_SliderGreen;
	[SerializeField] UIColorSlider m_SliderBlue;
	[SerializeField] UIColorSlider m_SliderAlpha;
	[SerializeField] TMP_Text      m_HexLabel;

	[SerializeField] Toggle m_BackgroundPrimaryToggle;
	[SerializeField] Toggle m_BackgroundSecondaryToggle;
	[SerializeField] Toggle m_ForegroundPrimaryToggle;
	[SerializeField] Toggle m_ForegroundSecondaryToggle;

	[SerializeField] Graphic m_BackgroundPrimary;
	[SerializeField] Graphic m_BackgroundSecondary;
	[SerializeField] Graphic m_ForegroundPrimary;
	[SerializeField] Graphic m_ForegroundSecondary;

	[Inject] MenuProcessor m_MenuProcessor;

	Action<Color, Color, Color, Color> m_Callback;

	Graphic m_Color;

	protected override void Awake()
	{
		base.Awake();
		
		m_SliderRed.OnValueChanged.AddListener(ProcessRed);
		m_SliderGreen.OnValueChanged.AddListener(ProcessGreen);
		m_SliderBlue.OnValueChanged.AddListener(ProcessBlue);
		m_SliderAlpha.OnValueChanged.AddListener(ProcessAlpha);
		
		m_BackgroundPrimaryToggle.onValueChanged.AddListener(BackgroundPrimaryMode);
		m_BackgroundSecondaryToggle.onValueChanged.AddListener(BackgroundSecondaryMode);
		m_ForegroundPrimaryToggle.onValueChanged.AddListener(ForegroundPrimaryMode);
		m_ForegroundSecondaryToggle.onValueChanged.AddListener(ForegroundSecondaryMode);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SliderRed.OnValueChanged.RemoveListener(ProcessRed);
		m_SliderGreen.OnValueChanged.RemoveListener(ProcessGreen);
		m_SliderBlue.OnValueChanged.RemoveListener(ProcessBlue);
		m_SliderAlpha.OnValueChanged.RemoveListener(ProcessAlpha);
		
		m_BackgroundPrimaryToggle.onValueChanged.RemoveListener(BackgroundPrimaryMode);
		m_BackgroundSecondaryToggle.onValueChanged.RemoveListener(BackgroundSecondaryMode);
		m_ForegroundPrimaryToggle.onValueChanged.RemoveListener(ForegroundPrimaryMode);
		m_ForegroundSecondaryToggle.onValueChanged.RemoveListener(ForegroundSecondaryMode);
	}

	public void Setup(
		Color                              _BackgroundPrimary,
		Color                              _BackgroundSecondary,
		Color                              _ForegroundPrimary,
		Color                              _ForegroundSecondary,
		Action<Color, Color, Color, Color> _Callback
	)
	{
		m_BackgroundPrimary.color   = _BackgroundPrimary;
		m_BackgroundSecondary.color = _BackgroundSecondary;
		m_ForegroundPrimary.color   = _ForegroundPrimary;
		m_ForegroundSecondary.color = _ForegroundSecondary;
		m_Callback                  = _Callback;
	}

	public async void Confirm()
	{
		Action<Color, Color, Color, Color> action = m_Callback;
		m_Callback = null;
		action?.Invoke(
			m_BackgroundPrimary.color,
			m_BackgroundSecondary.color,
			m_ForegroundPrimary.color,
			m_ForegroundSecondary.color
		);
		
		await m_MenuProcessor.Hide(MenuType.ColorMenu);
	}

	public async void Cancel()
	{
		await m_MenuProcessor.Hide(MenuType.ColorMenu);
	}

	public void Copy()
	{
		ProcessHex();
		
		GUIUtility.systemCopyBuffer = m_HexLabel.text;
	}

	public void Paste()
	{
		if (m_Color == null)
			return;
		
		string hex = GUIUtility.systemCopyBuffer;
		
		if (string.IsNullOrEmpty(hex))
			return;
		
		if (!hex.StartsWith("#"))
			hex = $"#{hex}";
		
		if (ColorUtility.TryParseHtmlString(hex, out Color color))
		{
			m_Color.color = color;
			
			ProcessMode();
		}
	}

	protected override void OnShowStarted()
	{
		m_BackgroundPrimaryToggle.SetIsOnWithoutNotify(true);
		
		m_Color = m_BackgroundPrimary;
		
		ProcessMode();
	}

	void BackgroundPrimaryMode(bool _Value)
	{
		if (_Value)
			m_Color = m_BackgroundPrimary;
		
		ProcessMode();
	}

	void BackgroundSecondaryMode(bool _Value)
	{
		if (_Value)
			m_Color = m_BackgroundSecondary;
		
		ProcessMode();
	}

	void ForegroundPrimaryMode(bool _Value)
	{
		if (_Value)
			m_Color = m_ForegroundPrimary;
		
		ProcessMode();
	}

	void ForegroundSecondaryMode(bool _Value)
	{
		if (_Value)
			m_Color = m_ForegroundSecondary;
		
		ProcessMode();
	}

	void ProcessRed(int _Value) => ProcessColor();

	void ProcessGreen(int _Value) => ProcessColor();

	void ProcessBlue(int _Value) => ProcessColor();

	void ProcessAlpha(int _Value) => ProcessColor();

	void ProcessColor()
	{
		if (m_Color == null)
			return;
		
		m_Color.color = new Color32(
			(byte)m_SliderRed.Value,
			(byte)m_SliderGreen.Value,
			(byte)m_SliderBlue.Value,
			(byte)m_SliderAlpha.Value
		);
		
		ProcessHex();
	}

	void ProcessMode()
	{
		Color32 color = m_Color != null ? m_Color.color : Color.white;
		
		m_SliderRed.Value   = color.r;
		m_SliderGreen.Value = color.g;
		m_SliderBlue.Value  = color.b;
		m_SliderAlpha.Value = color.a;
		
		ProcessHex();
	}

	void ProcessHex()
	{
		Color32 color = new Color32(
			(byte)m_SliderRed.Value,
			(byte)m_SliderGreen.Value,
			(byte)m_SliderBlue.Value,
			(byte)m_SliderAlpha.Value
		);
		
		string hex = ColorUtility.ToHtmlStringRGBA(color);
		
		m_HexLabel.text = hex;
	}
}