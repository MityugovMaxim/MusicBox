using UnityEngine.Scripting;

public class UIProfileItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProfileItem> { }
}