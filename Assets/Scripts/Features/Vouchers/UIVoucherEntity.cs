using Zenject;

public abstract class UIVoucherEntity : UIEntity
{
	public string VoucherID
	{
		get => m_VoucherID;
		set
		{
			if (m_VoucherID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_VoucherID))
				Unsubscribe();
			
			m_VoucherID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_VoucherID))
				Subscribe();
		}
	}

	protected VouchersManager VouchersManager => m_VouchersManager;

	[Inject] VouchersManager m_VouchersManager;

	string m_VoucherID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		VoucherID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}