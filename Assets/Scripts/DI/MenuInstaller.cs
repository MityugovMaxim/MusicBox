using Zenject;

public class MenuInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		UIMenu menu = GetComponent<UIMenu>();
		
		Container.BindInterfacesAndSelfTo(menu.GetType()).FromInstance(menu).AsSingle();
	}
}