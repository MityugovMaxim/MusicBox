using UnityEngine;

public class VouchersInstaller : FeatureInstaller
{
	[SerializeField] UIVoucherElement m_VoucherElement;

	public override void InstallBindings()
	{
		InstallSingleton<VouchersCollection>();
		
		InstallSingleton<ProfileVouchers>();
		
		InstallSingleton<VouchersManager>();
		
		InstallPool<UIVoucherElement, UIVoucherElement.Pool>(m_VoucherElement);
	}
}