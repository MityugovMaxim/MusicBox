using UnityEngine.Scripting;

[Preserve]
public class ComboProcessor : DataCollection<ComboSnapshot>
{
	protected override string Path => "combo";
}
