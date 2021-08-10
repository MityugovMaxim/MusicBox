using UnityEngine;

[CreateAssetMenu(fileName = "Menu Info", menuName = "Registry/Menu Info")]
public class MenuInfo : RegistryEntry
{
	public MenuType Type => m_Type;
	public string   Path => m_Path;

	[SerializeField]                       MenuType m_Type;
	[SerializeField, Path(typeof(UIMenu))] string   m_Path;
}