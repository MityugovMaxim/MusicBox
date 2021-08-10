using UnityEngine;

[CreateAssetMenu(fileName = "Menu Registry", menuName = "Registry/Menu Registry")]
public class MenuRegistry : Registry<MenuInfo>
{
	public override string Name => "Menus";
}