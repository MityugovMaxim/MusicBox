using Zenject;

public abstract class UIMainMenuPage : UIPage<MainMenuPageType>
{
	[Inject] LocalizationProcessor m_LocalizationProcessor;

	protected string GetLocalization(string _Key)
	{
		return m_LocalizationProcessor.Get(_Key);
	}

	protected string GetLocalization(string _Key, params object[] _Args)
	{
		if (_Args == null)
			return m_LocalizationProcessor.Get(_Key);
		else if (_Args.Length == 1)
			return m_LocalizationProcessor.Format(_Key, _Args[0]);
		else if (_Args.Length == 2)
			return m_LocalizationProcessor.Format(_Key, _Args[0], _Args[1]);
		else if (_Args.Length == 3)
			return m_LocalizationProcessor.Format(_Key, _Args[0], _Args[1], _Args[2]);
		else
			return m_LocalizationProcessor.Format(_Key, _Args);
	}
}