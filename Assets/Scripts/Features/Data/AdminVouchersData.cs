public class AdminVouchersData : AdminDatabaseData
{
	protected override string Path => "vouchers";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Vouchers"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "voucher_{0}"),
		new AdminCollectionAttribute("{voucher_id}/ids", "id_{0}"),
		new AdminFixedAttribute("{voucher_id}/{value}"),
		new AdminEnumAttribute<VoucherType>("{voucher_id}/type"),
		new AdminEnumAttribute<VoucherGroup>("{voucher_id}/group"),
		new AdminTickAttribute("{voucher_id}/amount", 1, 10, 100),
		new AdminLimitAttribute("{voucher_id}/amount", 0, decimal.MaxValue),
		new AdminDateAttribute("{voucher_id}/start_timestamp"),
		new AdminDateAttribute("{voucher_id}/end_timestamp"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{voucher_id}"))
			return CreateVoucher(_Node, _Path);
		if (AdminUtility.Match(_Path, "{voucher_id}/ids/{id}"))
			return CreateID(_Node, _Path);
		return null;
	}

	AdminNode CreateVoucher(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/type", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/group", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/amount", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/start_timestamp", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/end_timestamp", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/ids", AdminNodeType.Object);
		return root;
	}

	AdminNode CreateID(AdminNode _Node, string _Path)
	{
		return AdminNode.Create(this, _Node, _Path, AdminNodeType.Boolean);
	}
}
