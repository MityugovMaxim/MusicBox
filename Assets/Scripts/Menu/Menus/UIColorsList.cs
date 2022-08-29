using System;
using System.Collections.Generic;
using Zenject;

public class UIColorsList : UISnapshotsList<ColorsSnapshot, ColorsElementEntity>
{
	[Inject] UIColorsElement.Pool m_Pool;

	Action<ColorsSnapshot> m_Select;

	public void Setup(List<ColorsSnapshot> _Snapshots, Action<ColorsSnapshot> _Select)
	{
		m_Select = _Select;
		
		base.Setup(_Snapshots);
	}

	protected override ColorsSnapshot CreateSnapshot()
	{
		return new ColorsSnapshot("NEW COLOR SCHEME", 0);
	}

	protected override ColorsElementEntity CreateEntity(ColorsSnapshot _Snapshot, Action<ColorsSnapshot> _Remove)
	{
		return new ColorsElementEntity(_Snapshot, m_Select, _Remove, m_Pool);
	}
}