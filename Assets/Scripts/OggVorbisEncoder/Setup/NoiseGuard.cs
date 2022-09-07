namespace OggVorbisEncoder.Setup
{

public class NoiseGuard
{
    public NoiseGuard(int _Low, int _High, int _Fix)
    {
        Low = _Low;
        High = _High;
        Fixed = _Fix;
    }

    public int Low { get; }
    public int High { get; }
    public int Fixed { get; }
}
}
