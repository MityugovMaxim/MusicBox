using UnityEngine.Scripting;

public class UIProfileElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProfileElement> { }
}
