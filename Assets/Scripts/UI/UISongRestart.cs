using System.Threading.Tasks;
using Zenject;

public class UISongRestart : UIEntity
{
	[Inject] SongController m_SongController;
	[Inject] MenuProcessor  m_MenuProcessor;

	SongMode m_Mode;

	public async void Restart()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_SongController.Restart();
		
		await Task.WhenAll(
			m_MenuProcessor.Hide(MenuType.PauseMenu),
			m_MenuProcessor.Hide(MenuType.ReviveMenu),
			m_MenuProcessor.Hide(MenuType.ResultMenu)
		);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}
}
