using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIBackgroundItem : UIGroup
{
	[Preserve]
	public class Factory : PlaceholderFactory<UIBackgroundItem, UIBackgroundItem> { }

	[SerializeField] WebImage m_Image;

	string m_Path;

	public void Setup(string _Path)
	{
		m_Path = _Path;
	}

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		await m_Image.Load(m_Path);
		
		await base.ShowAnimation(_Duration, _Instant, _Token);
	}
}