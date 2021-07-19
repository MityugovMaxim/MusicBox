using UnityEngine;

public class ColorProcessor : MonoBehaviour
{
	public ColorScheme DefaultColorScheme => m_DefaultColorScheme != null ? m_DefaultColorScheme.ColorScheme : default;

	public ColorScheme ColorScheme
	{
		get => m_ColorScheme;
		set
		{
			if (m_ColorScheme == value)
				return;
			
			m_ColorScheme = value;
			
			ProcessColors();
		}
	}

	static readonly int m_BackgroundPrimaryColorPropertyID   = Shader.PropertyToID("_BackgroundPrimaryColor");
	static readonly int m_BackgroundSecondaryColorPropertyID = Shader.PropertyToID("_BackgroundSecondaryColor");
	static readonly int m_ForegroundPrimaryColorPropertyID   = Shader.PropertyToID("_ForegroundPrimaryColor");
	static readonly int m_ForegroundSecondaryColorPropertyID = Shader.PropertyToID("_ForegroundSecondaryColor");

	[SerializeField] ColorSchemeAsset m_DefaultColorScheme;

	ColorScheme m_ColorScheme;

	void Awake()
	{
		ColorScheme = DefaultColorScheme;
	}

	#if UNITY_EDITOR
	void OnValidate()
	{
		ColorScheme = DefaultColorScheme;
	}
	#endif

	void ProcessColors()
	{
		Shader.SetGlobalColor(m_BackgroundPrimaryColorPropertyID, ColorScheme.BackgroundPrimary);
		Shader.SetGlobalColor(m_BackgroundSecondaryColorPropertyID, ColorScheme.BackgroundSecondary);
		Shader.SetGlobalColor(m_ForegroundPrimaryColorPropertyID, ColorScheme.ForegroundSecondary);
		Shader.SetGlobalColor(m_ForegroundSecondaryColorPropertyID, ColorScheme.ForegroundPrimary);
	}
}
