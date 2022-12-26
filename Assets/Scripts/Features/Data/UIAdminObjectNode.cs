public class UIAdminObjectNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	protected override void ValueChanged() => RefreshNodes();
}
