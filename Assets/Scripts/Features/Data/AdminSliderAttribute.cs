public class AdminSliderAttribute : AdminAttribute
{
	public float Min   { get; }
	public float Max   { get; }
	public int   Steps { get; }

	public AdminSliderAttribute(string _Path, float _Min, float _Max, int _Steps = 0) : base(_Path)
	{
		Min   = _Min;
		Max   = _Max;
		Steps = _Steps;
	}
}
