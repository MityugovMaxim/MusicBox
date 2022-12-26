using UnityEngine;
using Zenject;

public abstract class UIBadge : UIEntity
{
	protected BadgeManager BadgeManager => m_BadgeManager;

	[SerializeField] UIUnitLabel m_Value;
	[SerializeField] UIGroup     m_Content;

	[Inject] BadgeManager m_BadgeManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Preload();
		
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Unsubscribe();
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected virtual void Preload() => Process();

	protected abstract void Process();

	protected void SetValue(int _Value, bool _Instant = false)
	{
		bool instant = _Instant || !gameObject.activeInHierarchy;
		
		if (_Value > 0)
		{
			m_Value.Value = _Value;
			m_Content.Show(instant);
		}
		else
		{
			m_Content.Hide(instant);
		}
	}
}
