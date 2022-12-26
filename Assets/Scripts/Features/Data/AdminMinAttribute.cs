public class AdminMinAttribute : AdminLimitAttribute
{
	public AdminMinAttribute(string _Path, decimal _Max) : base(_Path, decimal.MinValue, _Max) { }
}