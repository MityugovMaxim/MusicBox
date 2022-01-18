public class LevelComboSignal
{
	public int   Combo      { get; }
	public int   Multiplier { get; }
	public float Progress   { get; }

	public LevelComboSignal(int _Combo, int _Multiplier, float _Progress)
	{
		Combo      = _Combo;
		Multiplier = _Multiplier;
		Progress   = _Progress;
	}
}