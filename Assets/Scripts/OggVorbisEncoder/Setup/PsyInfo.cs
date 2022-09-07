using System.Linq;

namespace OggVorbisEncoder.Setup
{

public class PsyInfo
{
    public const int BANDS = 17;
    private const int NoiseCompandLevels = 40;
    private const int NoiseCurves = 3;
    private float[] m_NoiseCompand;
    private float[][] m_NoiseOffset;
    private float[] m_ToneAtt;

    private float[] m_ToneMasterAtt;

    public PsyInfo(
        int _BlockFlag,
        float _AthAdjAtt,
        float _AthMaxAtt,
        float[] _ToneMasterAtt,
        float _ToneCenterBoost,
        float _ToneDecay,
        float _ToneAbsLimit,
        float[] _ToneAtt,
        int _NoiseMaskP,
        float _NoiseMaxSuppress,
        float _NoiseWindowLow,
        float _NoiseWindowHigh,
        int _NoiseWindowLowMin,
        int _NoiseWindowHighMin,
        int _NoiseWindowFixed,
        float[][] _NoiseOffset,
        float[] _NoiseCompand,
        float _MaxCurveDecibel,
        bool _Normalize,
        int _NormalStart,
        int _NormalPartition,
        double _NormalThreshold)
    {
        BlockFlag = _BlockFlag;
        AthAdjAtt = _AthAdjAtt;
        AthMaxAtt = _AthMaxAtt;
        ToneMasterAtt = _ToneMasterAtt;
        ToneCenterBoost = _ToneCenterBoost;
        ToneDecay = _ToneDecay;
        ToneAbsLimit = _ToneAbsLimit;
        ToneAtt = _ToneAtt;
        NoiseMaskP = _NoiseMaskP;
        NoiseMaxSuppress = _NoiseMaxSuppress;
        NoiseWindowLow = _NoiseWindowLow;
        NoiseWindowHigh = _NoiseWindowHigh;
        NoiseWindowLowMin = _NoiseWindowLowMin;
        NoiseWindowHighMin = _NoiseWindowHighMin;
        NoiseWindowFixed = _NoiseWindowFixed;
        NoiseOffset = _NoiseOffset;
        NoiseCompand = _NoiseCompand;
        MaxCurveDecibel = _MaxCurveDecibel;
        Normalize = _Normalize;
        NormalStart = _NormalStart;
        NormalPartition = _NormalPartition;
        NormalThreshold = _NormalThreshold;
    }

    public int BlockFlag { get; set; }

    public float AthAdjAtt { get; set; }
    public float AthMaxAtt { get; set; }

    public float[] ToneMasterAtt
    {
        get { return m_ToneMasterAtt; }
        private set { m_ToneMasterAtt = value.ToFixedLength(NoiseCurves); }
    }

    public float ToneCenterBoost { get; set; }
    public float ToneDecay { get; set; }
    public float ToneAbsLimit { get; set; }

    public float[] ToneAtt
    {
        get { return m_ToneAtt; }
        private set { m_ToneAtt = value.ToFixedLength(BANDS); }
    }

    public int NoiseMaskP { get; }
    public float NoiseMaxSuppress { get; set; }
    public float NoiseWindowLow { get; }
    public float NoiseWindowHigh { get; }
    public int NoiseWindowLowMin { get; set; }
    public int NoiseWindowHighMin { get; set; }
    public int NoiseWindowFixed { get; set; }

    public float[][] NoiseOffset
    {
        get { return m_NoiseOffset; }
        private set
        {
            var fixedValue = value.Select(_S => _S.ToFixedLength(BANDS).ToArray());
            m_NoiseOffset = fixedValue.ToArray().ToFixedLength(NoiseCurves);
        }
    }

    public float[] NoiseCompand
    {
        get { return m_NoiseCompand; }
        private set { m_NoiseCompand = value.ToFixedLength(NoiseCompandLevels); }
    }

    public float MaxCurveDecibel { get; set; }

    public bool Normalize { get; set; }
    public int NormalStart { get; set; }
    public int NormalPartition { get; set; }
    public double NormalThreshold { get; set; }

    public PsyInfo Clone() => new PsyInfo(
        BlockFlag,
        AthAdjAtt,
        AthMaxAtt,
        ToneMasterAtt.ToArray(),
        ToneCenterBoost,
        ToneDecay,
        ToneAbsLimit,
        ToneAtt.ToArray(),
        NoiseMaskP,
        NoiseMaxSuppress,
        NoiseWindowLow,
        NoiseWindowHigh,
        NoiseWindowLowMin,
        NoiseWindowHighMin,
        NoiseWindowFixed,
        NoiseOffset.Select(_S => _S.ToArray()).ToArray(),
        NoiseCompand.ToArray(),
        MaxCurveDecibel,
        Normalize,
        NormalStart,
        NormalPartition,
        NormalThreshold);
}
}
