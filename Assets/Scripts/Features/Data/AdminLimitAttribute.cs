public class AdminLimitAttribute : AdminAttribute
{
	public decimal Min { get; }
	public decimal Max { get; }

	public AdminLimitAttribute(string _Path, decimal _Min, decimal _Max) : base(_Path)
	{
		Min = _Min;
		Max = _Max;
	}
}
