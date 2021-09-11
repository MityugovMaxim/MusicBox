using UnityEngine;

[CreateAssetMenu(fileName = "Menu Info", menuName = "Registry/Menu Info")]
public class MenuInfo : RegistryEntry
{
	public MenuType Type => m_Type;
	public string   Path => m_Path;

	[SerializeField, HideInInspector]                       MenuType m_Type;
	[SerializeField, HideInInspector, Path(typeof(UIMenu))] string   m_Path;
}
