using UnityEngine;

[CreateAssetMenu(fileName = "Color Scheme", menuName = "Visuals/Color Scheme")]
public class ColorSchemeAsset : ScriptableObject
{
	public ColorScheme ColorScheme => m_ColorScheme;

	[SerializeField] ColorScheme m_ColorScheme;
}