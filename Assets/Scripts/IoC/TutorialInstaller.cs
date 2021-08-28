using UnityEngine;
using Zenject;

public class TutorialInstaller : MonoInstaller
{
	[SerializeField] TapResolver    m_TapResolver;
	[SerializeField] DoubleResolver m_DoubleResolver;
	[SerializeField] HoldResolver   m_HoldResolver;

	public override void InstallBindings()
	{
		Container.BindInterfacesTo<TapResolver>().FromInstance(m_TapResolver).AsSingle();
		Container.BindInterfacesTo<DoubleResolver>().FromInstance(m_DoubleResolver).AsSingle();
		Container.BindInterfacesTo<HoldResolver>().FromInstance(m_HoldResolver).AsSingle();
	}
}