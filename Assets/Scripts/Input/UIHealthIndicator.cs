using UnityEngine;
using Zenject;

public class UIHealthIndicator : UIOrder
{
	[SerializeField] UIHealthHandle[] m_Handles;

	[Inject] HealthController m_HealthController;

	int m_Health;

	protected override void Awake()
	{
		base.Awake();
		
		m_HealthController.OnDamage  += ProcessHealth;
		m_HealthController.OnRestore += ProcessHealth;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_HealthController.OnDamage  -= ProcessHealth;
		m_HealthController.OnRestore -= ProcessHealth;
	}

	void ProcessHealth(int _Health)
	{
		for (int i = 0; i < m_Handles.Length; i++)
		{
			UIHealthHandle handle = m_Handles[i];
			
			if (handle == null)
				continue;
			
			if (i < _Health)
				handle.Restore();
			else
				handle.Damage();
		}
	}
}
