using UnityEngine;

[CreateAssetMenu(fileName = "Menu Info", menuName = "Registry/Menu Info")]
public class MenuInfo : ScriptableObject
{
	public MenuType Type      => m_Type;
	public bool     Focusable => m_Focusable;
	public string   Path      => m_Path;

	[SerializeField, HideInInspector] MenuType m_Type;
	[SerializeField, HideInInspector] bool     m_Focusable;

	[SerializeField, HideInInspector, Path(typeof(UIMenu))] string m_Path;
}
