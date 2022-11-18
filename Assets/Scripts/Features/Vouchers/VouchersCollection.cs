using UnityEngine.Scripting;

[Preserve]
public class VouchersCollection : DataCollection<VoucherSnapshot>
{
	protected override string Path => "vouchers";
}
