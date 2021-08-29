using UnityEngine;

[CreateAssetMenu(fileName = "Color Scheme", menuName = "Visuals/Color Scheme")]
public class ColorSchemeAsset : ScriptableObject
{
	public ColorScheme ColorScheme
	{
		get => m_ColorScheme;
		set => m_ColorScheme = value;
	}

	[SerializeField] ColorScheme m_ColorScheme;

	public static implicit operator ColorScheme(ColorSchemeAsset _ColorSchemeAsset)
	{
		return _ColorSchemeAsset != null ? _ColorSchemeAsset.ColorScheme : default;
	}
}