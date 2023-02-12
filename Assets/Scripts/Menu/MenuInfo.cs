using UnityEngine;

[CreateAssetMenu(fileName = "Menu Info", menuName = "Registry/Menu Info")]
public class MenuInfo : ScriptableObject
{
	public MenuType Type      => m_Type;
	public MenuMode Mode      => m_Mode;
	public string   Path      => m_Path;

	[SerializeField, HideInInspector] MenuType m_Type;
	[SerializeField, HideInInspector] MenuMode m_Mode;
	[SerializeField, HideInInspector, Path(typeof(UIMenu))] string m_Path;
}
