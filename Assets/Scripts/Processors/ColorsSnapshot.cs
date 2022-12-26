using Firebase.Database;
using UnityEngine;

public class ColorsSnapshot : Snapshot
{
	public Color BackgroundPrimary   { get; set; }
	public Color BackgroundSecondary { get; set; }
	public Color ForegroundPrimary   { get; set; }
	public Color ForegroundSecondary { get; set; }

	public ColorsSnapshot(DataSnapshot _Data) : base(_Data)
	{
		BackgroundPrimary   = _Data.GetColor("background_primary");
		BackgroundSecondary = _Data.GetColor("background_secondary");
		ForegroundPrimary   = _Data.GetColor("foreground_primary");
		ForegroundSecondary = _Data.GetColor("foreground_secondary");
	}

	public static bool Equals(ColorsSnapshot _A, ColorsSnapshot _B)
	{
		if (_A == _B)
			return true;
		
		if (_A == null || _B == null)
			return false;
		
		return _A.BackgroundPrimary == _B.BackgroundPrimary &&
			_A.BackgroundSecondary == _B.BackgroundSecondary &&
			_A.ForegroundPrimary == _B.ForegroundPrimary &&
			_A.ForegroundSecondary == _B.ForegroundSecondary;
	}
}
