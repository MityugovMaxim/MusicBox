using System;
using System.Buffers;

namespace OggVorbisEncoder.Lookups
{

public class MdctLookup
{
    private const float Pi3Eighths = .38268343236508977175f;
    private const float Pi2Eighths = .70710678118654752441f;
    private const float Pi1Eighth = .92387953251128675613f;
    private readonly int[] m_BITReverse;
    private readonly int m_LOG2N;
    private readonly int m_N;

    private readonly float m_Scale;
    private readonly float[] m_Trig;

    public MdctLookup(int _N)
    {
        m_N = _N;
        var n2 = _N >> 1;
        m_LOG2N = (int)Math.Round(Math.Log(_N) / Math.Log(2f));
        m_Trig = new float[_N + _N / 4];
        m_BITReverse = new int[_N / 4];

        // trig lookups
        for (var i = 0; i < _N / 4; i++)
        {
            m_Trig[i * 2] = (float)Math.Cos(Math.PI / _N * (4 * i));
            m_Trig[i * 2 + 1] = (float)-Math.Sin(Math.PI / _N * (4 * i));
            m_Trig[n2 + i * 2] = (float)Math.Cos(Math.PI / (2 * _N) * (2 * i + 1));
            m_Trig[n2 + i * 2 + 1] = (float)Math.Sin(Math.PI / (2 * _N) * (2 * i + 1));
        }

        for (var i = 0; i < _N / 8; i++)
        {
            m_Trig[_N + i * 2] = (float)(Math.Cos(Math.PI / _N * (4 * i + 2)) * .5);
            m_Trig[_N + i * 2 + 1] = (float)(-Math.Sin(Math.PI / _N * (4 * i + 2)) * .5);
        }

        // bitreverse lookup
        var mask = (1 << (m_LOG2N - 1)) - 1;
        var msb = 1 << (m_LOG2N - 2);
        for (var i = 0; i < _N / 8; i++)
        {
            var acc = 0;
            for (var j = 0; msb >> j != 0; j++)
                if (((msb >> j) & i) != 0)
                    acc |= 1 << j;

            m_BITReverse[i * 2] = (~acc & mask) - 1;
            m_BITReverse[i * 2 + 1] = acc;
        }

        m_Scale = 4f / _N;
    }

    public void Forward(in Span<float> _Input, in Span<float> _Output)
    {
        var n = m_N;
        var n2 = n >> 1;
        var n4 = n >> 2;
        var n8 = n >> 3;
        var workArr = ArrayPool<float>.Shared.Rent(n);
        var work = new Span<float>(workArr, 0, n);
        var w2 = work.Slice(n2);

        // rotate 
        // window + rotate + step 1 
        var x0 = n2 + n4;
        var x1 = x0 + 1;
        var t = n2;

        var i = 0;
        for (; i < n8; i += 2)
        {
            x0 -= 4;
            t -= 2;
            var r0 = _Input[x0 + 2] + _Input[x1 + 0];
            var r1 = _Input[x0 + 0] + _Input[x1 + 2];
            w2[i] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            w2[i + 1] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];
            x1 += 4;
        }

        x1 = 1;

        for (; i < n2 - n8; i += 2)
        {
            t -= 2;
            x0 -= 4;
            var r0 = _Input[x0 + 2] - _Input[x1 + 0];
            var r1 = _Input[x0 + 0] - _Input[x1 + 2];
            w2[i] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            w2[i + 1] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];
            x1 += 4;
        }

        x0 = n;

        for (; i < n2; i += 2)
        {
            t -= 2;
            x0 -= 4;
            var r0 = -_Input[x0 + 2] - _Input[x1 + 0];
            var r1 = -_Input[x0 + 0] - _Input[x1 + 2];
            w2[i] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            w2[i + 1] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];
            x1 += 4;
        }


        Butterflies(w2, n2);
        ReverseBits(work);

        // rotate + window 
        t = n2;
        x0 = n2;
        var offset = 0;
        for (i = 0; i < n4; i++)
        {
            x0--;
            _Output[i] = (work[offset + 0] * m_Trig[t + 0]
                         + work[offset + 1] * m_Trig[t + 1]) * m_Scale;

            _Output[x0 + 0] = (work[offset + 0] * m_Trig[t + 1]
                              - work[offset + 1] * m_Trig[t + 0]) * m_Scale;
            offset += 2;
            t += 2;
        }

        ArrayPool<float>.Shared.Return(workArr);
    }

    private void Butterflies(in Span<float> _Data, int _Points)
    {
        var stages = m_LOG2N - 5;

        if (--stages > 0)
            ButterflyFirst(_Data, _Points);

        for (var i = 1; --stages > 0; i++)
            for (var j = 0; j < 1 << i; j++)
                ButterflyGeneric(_Data, (_Points >> i) * j, _Points >> i, 4 << i);

        for (var j = 0; j < _Points; j += 32)
            Butterfly32(_Data, j);
    }

    private static void Butterfly32(in Span<float> _Data, int _Offset)
    {
        var r0 = _Data[_Offset + 30] - _Data[_Offset + 14];
        var r1 = _Data[_Offset + 31] - _Data[_Offset + 15];

        _Data[_Offset + 30] += _Data[_Offset + 14];
        _Data[_Offset + 31] += _Data[_Offset + 15];
        _Data[_Offset + 14] = r0;
        _Data[_Offset + 15] = r1;

        r0 = _Data[_Offset + 28] - _Data[_Offset + 12];
        r1 = _Data[_Offset + 29] - _Data[_Offset + 13];
        _Data[_Offset + 28] += _Data[_Offset + 12];
        _Data[_Offset + 29] += _Data[_Offset + 13];
        _Data[_Offset + 12] = r0 * Pi1Eighth - r1 * Pi3Eighths;
        _Data[_Offset + 13] = r0 * Pi3Eighths + r1 * Pi1Eighth;

        r0 = _Data[_Offset + 26] - _Data[_Offset + 10];
        r1 = _Data[_Offset + 27] - _Data[_Offset + 11];
        _Data[_Offset + 26] += _Data[_Offset + 10];
        _Data[_Offset + 27] += _Data[_Offset + 11];
        _Data[_Offset + 10] = (r0 - r1) * Pi2Eighths;
        _Data[_Offset + 11] = (r0 + r1) * Pi2Eighths;

        r0 = _Data[_Offset + 24] - _Data[_Offset + 8];
        r1 = _Data[_Offset + 25] - _Data[_Offset + 9];
        _Data[_Offset + 24] += _Data[_Offset + 8];
        _Data[_Offset + 25] += _Data[_Offset + 9];
        _Data[_Offset + 8] = r0 * Pi3Eighths - r1 * Pi1Eighth;
        _Data[_Offset + 9] = r1 * Pi3Eighths + r0 * Pi1Eighth;

        r0 = _Data[_Offset + 22] - _Data[_Offset + 6];
        r1 = _Data[_Offset + 7] - _Data[_Offset + 23];
        _Data[_Offset + 22] += _Data[_Offset + 6];
        _Data[_Offset + 23] += _Data[_Offset + 7];
        _Data[_Offset + 6] = r1;
        _Data[_Offset + 7] = r0;

        r0 = _Data[_Offset + 4] - _Data[_Offset + 20];
        r1 = _Data[_Offset + 5] - _Data[_Offset + 21];
        _Data[_Offset + 20] += _Data[_Offset + 4];
        _Data[_Offset + 21] += _Data[_Offset + 5];
        _Data[_Offset + 4] = r1 * Pi1Eighth + r0 * Pi3Eighths;
        _Data[_Offset + 5] = r1 * Pi3Eighths - r0 * Pi1Eighth;

        r0 = _Data[_Offset + 2] - _Data[_Offset + 18];
        r1 = _Data[_Offset + 3] - _Data[_Offset + 19];
        _Data[_Offset + 18] += _Data[_Offset + 2];
        _Data[_Offset + 19] += _Data[_Offset + 3];
        _Data[_Offset + 2] = (r1 + r0) * Pi2Eighths;
        _Data[_Offset + 3] = (r1 - r0) * Pi2Eighths;

        r0 = _Data[_Offset + 0] - _Data[_Offset + 16];
        r1 = _Data[_Offset + 1] - _Data[_Offset + 17];
        _Data[_Offset + 16] += _Data[_Offset + 0];
        _Data[_Offset + 17] += _Data[_Offset + 1];
        _Data[_Offset + 0] = r1 * Pi3Eighths + r0 * Pi1Eighth;
        _Data[_Offset + 1] = r1 * Pi1Eighth - r0 * Pi3Eighths;

        Butterfly16(_Data, _Offset);
        Butterfly16(_Data, _Offset + 16);
    }

    private static void Butterfly16(in Span<float> _Data, int _Offset)
    {
        var r0 = _Data[_Offset + 1] - _Data[_Offset + 9];
        var r1 = _Data[_Offset + 0] - _Data[_Offset + 8];

        _Data[_Offset + 8] += _Data[_Offset + 0];
        _Data[_Offset + 9] += _Data[_Offset + 1];
        _Data[_Offset + 0] = (r0 + r1) * Pi2Eighths;
        _Data[_Offset + 1] = (r0 - r1) * Pi2Eighths;

        r0 = _Data[_Offset + 3] - _Data[_Offset + 11];
        r1 = _Data[_Offset + 10] - _Data[_Offset + 2];
        _Data[_Offset + 10] += _Data[_Offset + 2];
        _Data[_Offset + 11] += _Data[_Offset + 3];
        _Data[_Offset + 2] = r0;
        _Data[_Offset + 3] = r1;

        r0 = _Data[_Offset + 12] - _Data[_Offset + 4];
        r1 = _Data[_Offset + 13] - _Data[_Offset + 5];
        _Data[_Offset + 12] += _Data[_Offset + 4];
        _Data[_Offset + 13] += _Data[_Offset + 5];
        _Data[_Offset + 4] = (r0 - r1) * Pi2Eighths;
        _Data[_Offset + 5] = (r0 + r1) * Pi2Eighths;

        r0 = _Data[_Offset + 14] - _Data[_Offset + 6];
        r1 = _Data[_Offset + 15] - _Data[_Offset + 7];
        _Data[_Offset + 14] += _Data[_Offset + 6];
        _Data[_Offset + 15] += _Data[_Offset + 7];
        _Data[_Offset + 6] = r0;
        _Data[_Offset + 7] = r1;

        Butterfly8(_Data, _Offset);
        Butterfly8(_Data, _Offset + 8);
    }

    private static void Butterfly8(in Span<float> _Data, int _Offset)
    {
        var r0 = _Data[_Offset + 6] + _Data[_Offset + 2];
        var r1 = _Data[_Offset + 6] - _Data[_Offset + 2];
        var r2 = _Data[_Offset + 4] + _Data[_Offset + 0];
        var r3 = _Data[_Offset + 4] - _Data[_Offset + 0];

        _Data[_Offset + 6] = r0 + r2;
        _Data[_Offset + 4] = r0 - r2;

        r0 = _Data[_Offset + 5] - _Data[_Offset + 1];
        r2 = _Data[_Offset + 7] - _Data[_Offset + 3];
        _Data[_Offset + 0] = r1 + r0;
        _Data[_Offset + 2] = r1 - r0;

        r0 = _Data[_Offset + 5] + _Data[_Offset + 1];
        r1 = _Data[_Offset + 7] + _Data[_Offset + 3];
        _Data[_Offset + 3] = r2 + r3;
        _Data[_Offset + 1] = r2 - r3;
        _Data[_Offset + 7] = r1 + r0;
        _Data[_Offset + 5] = r1 - r0;
    }

    private void ButterflyGeneric(in Span<float> _Data, int _Offset, int _Points, int _TrigIncrement)
    {
        var t = 0;
        var x1 = _Offset + _Points - 8;
        var x2 = _Offset + (_Points >> 1) - 8;

        do
        {
            var r0 = _Data[x1 + 6] - _Data[x2 + 6];
            var r1 = _Data[x1 + 7] - _Data[x2 + 7];
            _Data[x1 + 6] += _Data[x2 + 6];
            _Data[x1 + 7] += _Data[x2 + 7];
            _Data[x2 + 6] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            _Data[x2 + 7] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];

            t += _TrigIncrement;

            r0 = _Data[x1 + 4] - _Data[x2 + 4];
            r1 = _Data[x1 + 5] - _Data[x2 + 5];
            _Data[x1 + 4] += _Data[x2 + 4];
            _Data[x1 + 5] += _Data[x2 + 5];
            _Data[x2 + 4] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            _Data[x2 + 5] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];

            t += _TrigIncrement;

            r0 = _Data[x1 + 2] - _Data[x2 + 2];
            r1 = _Data[x1 + 3] - _Data[x2 + 3];
            _Data[x1 + 2] += _Data[x2 + 2];
            _Data[x1 + 3] += _Data[x2 + 3];
            _Data[x2 + 2] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            _Data[x2 + 3] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];

            t += _TrigIncrement;

            r0 = _Data[x1 + 0] - _Data[x2 + 0];
            r1 = _Data[x1 + 1] - _Data[x2 + 1];
            _Data[x1 + 0] += _Data[x2 + 0];
            _Data[x1 + 1] += _Data[x2 + 1];
            _Data[x2 + 0] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            _Data[x2 + 1] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];

            t += _TrigIncrement;
            x1 -= 8;
            x2 -= 8;
        } while (x2 >= _Offset);
    }

    private void ButterflyFirst(in Span<float> _Data, int _Points)
    {
        var x1 = _Points - 8;
        var x2 = (_Points >> 1) - 8;
        var t = 0;

        do
        {
            var r0 = _Data[x1 + 6] - _Data[x2 + 6];
            var r1 = _Data[x1 + 7] - _Data[x2 + 7];
            _Data[x1 + 6] += _Data[x2 + 6];
            _Data[x1 + 7] += _Data[x2 + 7];
            _Data[x2 + 6] = r1 * m_Trig[t + 1] + r0 * m_Trig[t + 0];
            _Data[x2 + 7] = r1 * m_Trig[t + 0] - r0 * m_Trig[t + 1];

            r0 = _Data[x1 + 4] - _Data[x2 + 4];
            r1 = _Data[x1 + 5] - _Data[x2 + 5];
            _Data[x1 + 4] += _Data[x2 + 4];
            _Data[x1 + 5] += _Data[x2 + 5];
            _Data[x2 + 4] = r1 * m_Trig[t + 5] + r0 * m_Trig[t + 4];
            _Data[x2 + 5] = r1 * m_Trig[t + 4] - r0 * m_Trig[t + 5];

            r0 = _Data[x1 + 2] - _Data[x2 + 2];
            r1 = _Data[x1 + 3] - _Data[x2 + 3];
            _Data[x1 + 2] += _Data[x2 + 2];
            _Data[x1 + 3] += _Data[x2 + 3];
            _Data[x2 + 2] = r1 * m_Trig[t + 9] + r0 * m_Trig[t + 8];
            _Data[x2 + 3] = r1 * m_Trig[t + 8] - r0 * m_Trig[t + 9];

            r0 = _Data[x1 + 0] - _Data[x2 + 0];
            r1 = _Data[x1 + 1] - _Data[x2 + 1];
            _Data[x1 + 0] += _Data[x2 + 0];
            _Data[x1 + 1] += _Data[x2 + 1];
            _Data[x2 + 0] = r1 * m_Trig[t + 13] + r0 * m_Trig[t + 12];
            _Data[x2 + 1] = r1 * m_Trig[t + 12] - r0 * m_Trig[t + 13];

            x1 -= 8;
            x2 -= 8;
            t += 16;
        } while (x2 >= 0);
    }

    private void ReverseBits(in Span<float> _Data)
    {
        var n = m_N;
        var bit = 0;
        var w0 = 0;
        // ReSharper disable once UselessBinaryOperation
        var w1 = w0 + (n >> 1);
        var x = w1;
        var t = n;

        do
        {
            var x0 = x + m_BITReverse[bit + 0];
            var x1 = x + m_BITReverse[bit + 1];

            var r0 = _Data[x0 + 1] - _Data[x1 + 1];
            var r1 = _Data[x0 + 0] + _Data[x1 + 0];
            var r2 = r1 * m_Trig[t + 0] + r0 * m_Trig[t + 1];
            var r3 = r1 * m_Trig[t + 1] - r0 * m_Trig[t + 0];

            w1 -= 4;

            r0 = 0.5f * (_Data[x0 + 1] + _Data[x1 + 1]);
            r1 = 0.5f * (_Data[x0 + 0] - _Data[x1 + 0]);

            _Data[w0 + 0] = r0 + r2;
            _Data[w1 + 2] = r0 - r2;
            _Data[w0 + 1] = r1 + r3;
            _Data[w1 + 3] = r3 - r1;

            x0 = x + m_BITReverse[bit + 2];
            x1 = x + m_BITReverse[bit + 3];

            r0 = _Data[x0 + 1] - _Data[x1 + 1];
            r1 = _Data[x0 + 0] + _Data[x1 + 0];
            r2 = r1 * m_Trig[t + 2] + r0 * m_Trig[t + 3];
            r3 = r1 * m_Trig[t + 3] - r0 * m_Trig[t + 2];

            r0 = 0.5f * (_Data[x0 + 1] + _Data[x1 + 1]);
            r1 = 0.5f * (_Data[x0 + 0] - _Data[x1 + 0]);

            _Data[w0 + 2] = r0 + r2;
            _Data[w1 + 0] = r0 - r2;
            _Data[w0 + 3] = r1 + r3;
            _Data[w1 + 1] = r3 - r1;

            t += 4;
            bit += 4;
            w0 += 4;
        } while (w0 < w1);
    }
}
}
