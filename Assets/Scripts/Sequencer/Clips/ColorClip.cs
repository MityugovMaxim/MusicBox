using UnityEngine;

public class ColorClip : Clip
{
	public ColorScheme ColorScheme => m_ColorScheme != null ? m_ColorScheme.ColorScheme : default;

	[SerializeField] ColorSchemeAsset m_ColorScheme;

	Track          m_Track;
	ColorProcessor m_ColorProcessor;
	ColorScheme    m_SourceColorScheme;

	#if UNITY_EDITOR
	void OnValidate()
	{
		if (Application.isEditor && !Application.isPlaying && m_Track != null)
			m_Track.Initialize(Sequencer);
	}
	#endif

	public void Initialize(
		Sequencer      _Sequencer,
		Track          _Track,
		ColorProcessor _ColorProcessor,
		ColorScheme    _ColorScheme
	)
	{
		base.Initialize(_Sequencer);
		
		m_Track             = _Track;
		m_ColorProcessor    = _ColorProcessor;
		m_SourceColorScheme = _ColorScheme;
	}

	protected override void OnEnter(float _Time)
	{
		if (m_ColorProcessor == null)
			return;
		
		ProcessColor(_Time);
	}

	protected override void OnUpdate(float _Time)
	{
		if (m_ColorProcessor == null)
			return;
		
		ProcessColor(_Time);
	}

	protected override void OnExit(float _Time)
	{
		if (m_ColorProcessor == null)
			return;
		
		ProcessColor(_Time);
	}

	void ProcessColor(float _Time)
	{
		float phase = GetNormalizedTime(_Time);
		
		ColorScheme colorScheme = ColorScheme.Lerp(m_SourceColorScheme, ColorScheme, phase);
		
		m_ColorProcessor.ColorScheme = colorScheme;
	}
}