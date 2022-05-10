public abstract class UIResultMenuPage : UIPage<ResultMenuPageType>
{
	public abstract bool Valid { get; }

	public abstract void Setup(string _SongID);

	public abstract void Play();
}