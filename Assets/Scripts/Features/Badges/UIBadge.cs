using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public abstract class UIBadge : UIEntity
{
	protected BadgeManager BadgeManager => m_BadgeManager;

	[SerializeField] TMP_Text m_Label;
	[SerializeField] UIGroup  m_Content;

	[Inject] BadgeManager m_BadgeManager;

	static readonly Dictionary<string, bool> m_Cache = new Dictionary<string, bool>();

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Process();
		
		Unsubscribe();
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void Process();

	protected void SetValue(int _Value)
	{
		bool instant = !gameObject.activeInHierarchy;
		
		if (_Value > 0)
		{
			m_Label.text = _Value.ToString();
			m_Content.Show(instant);
		}
		else
		{
			m_Content.Hide(instant);
		}
	}
}
