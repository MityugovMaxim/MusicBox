namespace OggVorbisEncoder.Setup
{

public class Att3
{
    public Att3(int[] _Att, float _Boost, float _Decay)
    {
        Att = _Att;
        Boost = _Boost;
        Decay = _Decay;
    }

    public int[] Att { get; }
    public float Boost { get; }
    public float Decay { get; }
}
}
