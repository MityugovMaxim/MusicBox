using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ProfileDiscsParameter : ProfileParameter<DiscsObject>, IDataObject
{
	protected override string Name => "discs";

	protected override DiscsObject Create(DataSnapshot _Data) => new DiscsObject(_Data);

	public int GetBronze() => Value?.Bronze ?? 0;

	public int GetSilver() => Value?.Silver ?? 0;

	public int GetGold() => Value?.Gold ?? 0;

	public int GetPlatinum() => Value?.Platinum ?? 0;
}
