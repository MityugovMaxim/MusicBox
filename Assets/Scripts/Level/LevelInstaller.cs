using Zenject;

public class LevelInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		Container.Bind<Sequencer>().FromComponentOnRoot().AsSingle();
		Container.Bind<AudioProcessor>().FromComponentsInChildren().AsSingle();
		Container.Bind<ColorProcessor>().FromComponentInChildren().AsSingle();
	}
}