using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

[Menu(MenuType.ColorMenu)]
public class UIColorMenu : UIMenu
{
	[SerializeField] UIColorSlider m_SliderRed;
	[SerializeField] UIColorSlider m_SliderGreen;
	[SerializeField] UIColorSlider m_SliderBlue;
	[SerializeField] UIColorSlider m_SliderAlpha;
	[SerializeField] TMP_Text      m_HexLabel;
	[SerializeField] UIColorScheme m_Scheme;
	[SerializeField] RectTransform m_SchemeContainer;

	[SerializeField] Toggle m_BackgroundPrimaryToggle;
	[SerializeField] Toggle m_BackgroundSecondaryToggle;
	[SerializeField] Toggle m_ForegroundPrimaryToggle;
	[SerializeField] Toggle m_ForegroundSecondaryToggle;

	[SerializeField] Graphic    m_BackgroundPrimary;
	[SerializeField] Graphic    m_BackgroundSecondary;
	[SerializeField] Graphic    m_ForegroundPrimary;
	[SerializeField] Graphic    m_ForegroundSecondary;

	[Inject] MenuProcessor m_MenuProcessor;

	ASFColorTrack m_ColorTrack;

	Action<Color, Color, Color, Color> m_Callback;

	Graphic m_Color;

	readonly List<UIColorScheme> m_Schemes = new List<UIColorScheme>();

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

	public void Setup(ASFColorClip _Clip, Action<Color, Color, Color, Color> _Callback)
	{
		m_BackgroundPrimary.color   = _Clip.BackgroundPrimary;
		m_BackgroundSecondary.color = _Clip.BackgroundSecondary;
		m_ForegroundPrimary.color   = _Clip.ForegroundPrimary;
		m_ForegroundSecondary.color = _Clip.ForegroundSecondary;
		m_Callback                  = _Callback;
		
		UISongEditMenu songEditMenu = m_MenuProcessor.GetMenu<UISongEditMenu>();
		if (songEditMenu == null)
			return;
		
		m_ColorTrack = songEditMenu.Player.GetTrack<ASFColorTrack>();
		
		foreach (UIColorScheme scheme in m_Schemes)
			DestroyImmediate(scheme.gameObject);
		m_Schemes.Clear();
		
		m_ColorTrack.SortClips();
		
		foreach (ASFColorClip clip in m_ColorTrack.Clips)
		{
			UIColorScheme scheme = Instantiate(m_Scheme, m_SchemeContainer, false);
			
			scheme.Setup(
				_Clip == clip,
				clip.BackgroundPrimary,
				clip.BackgroundSecondary,
				clip.ForegroundPrimary,
				clip.ForegroundSecondary,
				SelectScheme
			);
			
			m_Schemes.Add(scheme);
		}
	}

	public void RandomSchemeA()
	{
		m_BackgroundPrimary.color   = RandomColor();
		m_BackgroundSecondary.color = RandomColor();
		m_ForegroundPrimary.color   = new Color(1, 1, 1, 0.75f);
		m_ForegroundSecondary.color = m_BackgroundPrimary.color;
		
		ProcessMode();
	}

	public void RandomSchemeB()
	{
		m_BackgroundPrimary.color   = RandomColor();
		m_BackgroundSecondary.color = RandomColor();
		m_ForegroundPrimary.color   = new Color(1, 1, 1, 0.75f);
		m_ForegroundSecondary.color = m_BackgroundSecondary.color;
		
		ProcessMode();
	}

	static Color RandomColor()
	{
		return Color.HSVToRGB(
			Random.value,
			Random.value,
			1
		);
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

	void SelectScheme(Color _BackgroundPrimary, Color _BackgroundSecondary, Color _ForegroundPrimary, Color _ForegroundSecondary)
	{
		m_BackgroundPrimary.color   = _BackgroundPrimary;
		m_BackgroundSecondary.color = _BackgroundSecondary;
		m_ForegroundPrimary.color   = _ForegroundPrimary;
		m_ForegroundSecondary.color = _ForegroundSecondary;
		
		ProcessMode();
	}

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
		
		ProcessScheme();
		
		ProcessHex();
	}

	void ProcessMode()
	{
		Color32 color = m_Color != null ? m_Color.color : Color.white;
		
		m_SliderRed.Value   = color.r;
		m_SliderGreen.Value = color.g;
		m_SliderBlue.Value  = color.b;
		m_SliderAlpha.Value = color.a;
		
		ProcessScheme();
		
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

	void ProcessScheme()
	{
		UIColorScheme scheme = m_Schemes.FirstOrDefault(_Scheme => _Scheme.Marker);
		if (scheme != null)
		{
			scheme.Setup(
				true,
				m_BackgroundPrimary.color,
				m_BackgroundSecondary.color,
				m_ForegroundPrimary.color,
				m_ForegroundSecondary.color,
				SelectScheme
			);
		}
		
		IASFColorSampler sampler = m_ColorTrack.Context as IASFColorSampler;
		
		if (sampler == null)
			return;
		
		ASFColorClip clip = new ASFColorClip(
			0,
			m_BackgroundPrimary.color,
			m_BackgroundSecondary.color,
			m_ForegroundPrimary.color,
			m_ForegroundSecondary.color
		);
		
		sampler.Sample(clip, clip, 1);
	}
}