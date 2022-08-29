using System.Collections.Generic;
using AudioBox.Compression;
using Firebase.Database;
using UnityEngine;

public class ColorsSnapshot : Snapshot
{
	public Color BackgroundPrimary   { get; set; }
	public Color BackgroundSecondary { get; set; }
	public Color ForegroundPrimary   { get; set; }
	public Color ForegroundSecondary { get; set; }

	public ColorsSnapshot(string _ID, int _Order) : base(_ID, _Order)
	{
		BackgroundPrimary   = Color.white;
		BackgroundSecondary = Color.white;
		ForegroundPrimary   = Color.white;
		ForegroundSecondary = Color.white;
	}

	public ColorsSnapshot(
		string _ID,
		int    _Order,
		Color  _BackgroundPrimary,
		Color  _BackgroundSecondary,
		Color  _ForegroundPrimary,
		Color  _ForegroundSecondary
	) : base(_ID, _Order)
	{
		BackgroundPrimary   = _BackgroundPrimary;
		BackgroundSecondary = _BackgroundSecondary;
		ForegroundPrimary   = _ForegroundPrimary;
		ForegroundSecondary = _ForegroundSecondary;
	}

	public ColorsSnapshot(DataSnapshot _Data) : base(_Data)
	{
		BackgroundPrimary   = _Data.GetColor("background_primary");
		BackgroundSecondary = _Data.GetColor("background_secondary");
		ForegroundPrimary   = _Data.GetColor("foreground_primary");
		ForegroundSecondary = _Data.GetColor("foreground_secondary");
	}

	public ColorsSnapshot(string _ID, IDictionary<string, object> _Data) : base(_ID, 0)
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

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["background_primary"]   = "#" + ColorUtility.ToHtmlStringRGBA(BackgroundPrimary);
		_Data["background_secondary"] = "#" + ColorUtility.ToHtmlStringRGBA(BackgroundSecondary);
		_Data["foreground_primary"]   = "#" + ColorUtility.ToHtmlStringRGBA(ForegroundPrimary);
		_Data["foreground_secondary"] = "#" + ColorUtility.ToHtmlStringRGBA(ForegroundSecondary);
	}
}