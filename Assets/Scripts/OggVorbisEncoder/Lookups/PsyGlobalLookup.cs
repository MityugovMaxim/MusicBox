using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{

public class PsyGlobalLookup
{
    private const int NegativeInfinite = -9999;
    private readonly PsyGlobal m_PsyGlobal;
    private float m_AmpMax;

    public PsyGlobalLookup(PsyGlobal _Global)
    {
        m_PsyGlobal = _Global;
        AmpMax = NegativeInfinite;
    }

    public float AmpMax
    {
        get { return m_AmpMax; }
        private set { m_AmpMax = Math.Max(NegativeInfinite, value); }
    }

    public void DecayAmpMax(int _N, int _SampleRate)
    {
        var secs = (float)_N / _SampleRate;
        AmpMax += secs * m_PsyGlobal.AmpMaxAttPerSec;
    }
}
}
