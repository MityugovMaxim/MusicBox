public class AdminMaxAttribute : AdminLimitAttribute
{
	public AdminMaxAttribute(string _Path, decimal _Min) : base(_Path, _Min, decimal.MaxValue) { }
}