using System;
using UnityEngine;
using UnityEngine.UI;

[Menu(MenuType.ColorsEditMenu)]
public class UIColorsEditMenu : UIMenu
{
	static readonly int m_BackgroundPrimaryPropertyID   = Shader.PropertyToID("_BackgroundPrimaryColor");
	static readonly int m_BackgroundSecondaryPropertyID = Shader.PropertyToID("_BackgroundSecondaryColor");
	static readonly int m_ForegroundPrimaryPropertyID   = Shader.PropertyToID("_ForegroundPrimaryColor");
	static readonly int m_ForegroundSecondaryPropertyID = Shader.PropertyToID("_ForegroundSecondaryColor");

	Color Base
	{
		get => m_Base.color;
		set => m_Base.color = value;
	}

	Color BackgroundPrimary
	{
		get => m_BackgroundPrimary.color;
		set => m_BackgroundPrimary.color = value;
	}

	Color BackgroundSecondary
	{
		get => m_BackgroundSecondary.color;
		set => m_BackgroundSecondary.color = value;
	}

	Color ForegroundPrimary
	{
		get => m_ForegroundPrimary.color;
		set => m_ForegroundPrimary.color = value;
	}

	Color ForegroundSecondary
	{
		get => m_ForegroundSecondary.color;
		set => m_ForegroundSecondary.color = value;
	}

	[SerializeField] UIStringField m_IDField;
	[SerializeField] Graphic       m_Base;
	[SerializeField] Graphic       m_BackgroundPrimary;
	[SerializeField] Graphic       m_BackgroundSecondary;
	[SerializeField] Graphic       m_ForegroundPrimary;
	[SerializeField] Graphic       m_ForegroundSecondary;

	[SerializeField] Button m_BackButton;
	[SerializeField] Button m_BaseButton;
	[SerializeField] Button m_RandomButton;
	[SerializeField] Button m_MonochromaticButton;
	[SerializeField] Button m_ComplementaryButton;
	[SerializeField] Button m_SplitComplementaryButton;
	[SerializeField] Button m_TriadButton;
	[SerializeField] Button m_SquareButton;
	[SerializeField] Button m_AnalogousButton;
	[SerializeField] Button m_ShadesButton;
	[SerializeField] Button m_GoldenRatioButton;

	ColorsSnapshot m_Snapshot;
	Action         m_Callback;

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.onClick.AddListener(Back);
		m_BaseButton.onClick.AddListener(GenerateBase);
		m_RandomButton.onClick.AddListener(GenerateRandom);
		m_MonochromaticButton.onClick.AddListener(GenerateMonochromatic);
		m_ComplementaryButton.onClick.AddListener(GenerateComplementary);
		m_SplitComplementaryButton.onClick.AddListener(GenerateSplitComplementary);
		m_TriadButton.onClick.AddListener(GenerateTriad);
		m_SquareButton.onClick.AddListener(GenerateSquare);
		m_AnalogousButton.onClick.AddListener(GenerateAnalogous);
		m_ShadesButton.onClick.AddListener(GenerateShades);
		m_GoldenRatioButton.onClick.AddListener(GenerateGoldenRatio);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_BaseButton.onClick.RemoveListener(GenerateBase);
		m_RandomButton.onClick.RemoveListener(GenerateRandom);
		m_MonochromaticButton.onClick.RemoveListener(GenerateMonochromatic);
		m_ComplementaryButton.onClick.RemoveListener(GenerateComplementary);
		m_SplitComplementaryButton.onClick.RemoveListener(GenerateSplitComplementary);
		m_TriadButton.onClick.RemoveListener(GenerateTriad);
		m_SquareButton.onClick.RemoveListener(GenerateSquare);
		m_AnalogousButton.onClick.RemoveListener(GenerateAnalogous);
		m_ShadesButton.onClick.RemoveListener(GenerateShades);
		m_GoldenRatioButton.onClick.RemoveListener(GenerateGoldenRatio);
	}

	public void Setup(ColorsSnapshot _Snapshot, Action _Callback)
	{
		m_Snapshot = _Snapshot;
		m_Callback = _Callback;
		
		m_IDField.Setup(m_Snapshot, nameof(m_Snapshot.ID));
		
		ProcessColors(
			m_Snapshot.BackgroundPrimary,
			m_Snapshot.BackgroundSecondary,
			m_Snapshot.ForegroundPrimary,
			m_Snapshot.ForegroundSecondary
		);
	}

	protected override void OnHideStarted()
	{
		m_Callback?.Invoke();
	}

	void Back()
	{
		Hide();
	}

	void GenerateBase()
	{
		Base = ColorGenerator.GenerateBase();
		
		GenerateMonochromatic();
	}

	void GenerateRandom() => ProcessColors(ColorGenerator.Generate());

	void GenerateMonochromatic() => ProcessColors(ColorGenerator.Monochromatic(Base));

	void GenerateComplementary() => ProcessColors(ColorGenerator.Complementary(Base));

	void GenerateSplitComplementary() => ProcessColors(ColorGenerator.SplitComplementary(Base));

	void GenerateTriad() => ProcessColors(ColorGenerator.Triad(Base));

	void GenerateSquare() => ProcessColors(ColorGenerator.Square(Base));

	void GenerateAnalogous() => ProcessColors(ColorGenerator.Analogous(Base));

	void GenerateShades() => ProcessColors(ColorGenerator.Shades(Base));

	void GenerateGoldenRatio() => ProcessColors(ColorGenerator.GoldenRatio(Base));

	void ProcessColors(params Color[] _Colors)
	{
		Base                = _Colors[0];
		BackgroundPrimary   = _Colors[0];
		BackgroundSecondary = _Colors[1];
		ForegroundPrimary   = _Colors[2];
		ForegroundSecondary = _Colors[3];
		
		m_Snapshot.BackgroundPrimary   = BackgroundPrimary;
		m_Snapshot.BackgroundSecondary = BackgroundSecondary;
		m_Snapshot.ForegroundPrimary   = ForegroundPrimary;
		m_Snapshot.ForegroundSecondary = ForegroundSecondary;
		
		Shader.SetGlobalColor(m_BackgroundPrimaryPropertyID, BackgroundPrimary);
		Shader.SetGlobalColor(m_BackgroundSecondaryPropertyID, BackgroundSecondary);
		Shader.SetGlobalColor(m_ForegroundPrimaryPropertyID, ForegroundPrimary);
		Shader.SetGlobalColor(m_ForegroundSecondaryPropertyID, ForegroundSecondary);
		
		m_Callback?.Invoke();
	}
}