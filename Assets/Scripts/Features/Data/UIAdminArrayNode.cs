public class UIAdminArrayNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	protected override void ValueChanged() => RefreshNodes();
}