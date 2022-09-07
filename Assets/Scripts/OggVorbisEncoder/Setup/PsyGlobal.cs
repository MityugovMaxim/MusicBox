using System.Linq;

namespace OggVorbisEncoder.Setup
{

public class PsyGlobal
{
    public const int ENVELOPE_BANDS = 7;
    public const int PACKET_BLOBS = 15;

    public PsyGlobal(
        int _EighthOctaveLines,
        float[] _PreEchoThreshold,
        float[] _PostEchoThreshold,
        float _StretchPenalty,
        float _PreEchoMinEnergy,
        float _AmpMaxAttPerSecond,
        int[] _CouplingPerKilohertz,
        int[][] _CouplingPointLimit,
        int[] _CouplingPrePointAmp,
        int[] _CouplingPostPointAmp,
        int[][] _SlidingLowPass)
    {
        EighthOctaveLines = _EighthOctaveLines;

        PreEchoThreshold = _PreEchoThreshold.ToFixedLength(ENVELOPE_BANDS);
        PostEchoThreshold = _PostEchoThreshold.ToFixedLength(ENVELOPE_BANDS);

        StretchPenalty = _StretchPenalty;
        PreEchoMinEnergy = _PreEchoMinEnergy;
        AmpMaxAttPerSec = _AmpMaxAttPerSecond;

        CouplingPerKilohertz = _CouplingPerKilohertz.ToFixedLength(PACKET_BLOBS);
        CouplingPrePointAmp = _CouplingPrePointAmp.ToFixedLength(PACKET_BLOBS);
        CouplingPostPointAmp = _CouplingPostPointAmp.ToFixedLength(PACKET_BLOBS);

        CouplingPointLimit = _CouplingPointLimit.Select(_S => _S.ToFixedLength(PACKET_BLOBS)).ToArray();
        SlidingLowPass = _SlidingLowPass.Select(_S => _S.ToFixedLength(PACKET_BLOBS)).ToArray();
    }

    public int EighthOctaveLines { get; }

    // for block long/short tuning; encode only 
    public float[] PreEchoThreshold { get; }
    public float[] PostEchoThreshold { get; }
    public float StretchPenalty { get; }
    public float PreEchoMinEnergy { get; }
    public float AmpMaxAttPerSec { get; set; }

    // channel coupling config 
    public int[] CouplingPerKilohertz { get; }
    public int[][] CouplingPointLimit { get; }
    public int[] CouplingPrePointAmp { get; set; }
    public int[] CouplingPostPointAmp { get; set; }
    public int[][] SlidingLowPass { get; }

    public PsyGlobal Clone() => new PsyGlobal(
        EighthOctaveLines,
        PreEchoThreshold.ToArray(),
        PostEchoThreshold.ToArray(),
        StretchPenalty,
        PreEchoMinEnergy,
        AmpMaxAttPerSec,
        CouplingPerKilohertz.ToArray(),
        CouplingPointLimit.Select(_S => _S.ToArray()).ToArray(),
        CouplingPrePointAmp.ToArray(),
        CouplingPostPointAmp.ToArray(),
        SlidingLowPass.Select(_S => _S.ToArray()).ToArray());
}
}
