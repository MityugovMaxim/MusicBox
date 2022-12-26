public class AdminTickAttribute : AdminAttribute
{
	public decimal[] Intervals { get; }

	public AdminTickAttribute(string _Path, params decimal[] _Intervals) : base(_Path)
	{
		Intervals = _Intervals;
	}
}
