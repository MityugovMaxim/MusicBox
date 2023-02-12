using System;
using System.Diagnostics.CodeAnalysis;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{

public class ResidueLookup
{
    private readonly CodeBook[][] m_PartitionBooks;
    private readonly CodeBook m_PhraseBook;
    private readonly ResidueEntry m_Residue;
    private readonly int m_Stages;

    public ResidueLookup(ResidueEntry _Residue, CodeBook[] _FullBooks)
    {
        m_Residue = _Residue;

        m_PhraseBook = _FullBooks[_Residue.GroupBook];

        var acc = 0;
        var maxstage = 0;

        m_PartitionBooks = new CodeBook[_Residue.Partitions][];

        for (var j = 0; j < m_PartitionBooks.Length; j++)
        {
            var stages = Encoding.Log(_Residue.SecondStages[j]);
            if (stages == 0)
                continue;

            if (stages > maxstage)
                maxstage = stages;

            m_PartitionBooks[j] = new CodeBook[stages];

            for (var k = 0; k < stages; k++)
                if ((_Residue.SecondStages[j] & (1 << k)) != 0)
                    m_PartitionBooks[j][k] = _FullBooks[_Residue.BookList[acc++]];
        }

        m_Stages = maxstage;
    }

    public int Forward(
        EncodeBuffer _Buffer,
        int _Pcmend,
        int[][] _Couples,
        bool[] _Nonzero,
        int _Channels,
        int[][] _Partword)
    {
        return m_Residue.ResidueType switch
        {
            ResidueType.One => Res1Forward(_Buffer, _Pcmend, _Couples, _Nonzero, _Channels, _Partword),
            ResidueType.Two => Res2Forward(_Buffer, _Pcmend, _Couples, _Nonzero, _Channels, _Partword),
            _ => throw new NotImplementedException(),
        };
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private int Res1Forward(EncodeBuffer _Buffer, int _PCMEnd, int[][] _Couples, bool[] _Nonzero, int _Channels,
        int[][] _Partword)
    {
        var used = 0;

        for (var i = 0; i < _Channels; i++)
        {
            if (_Nonzero[i])
                _Couples[used++] = _Couples[i];
        }

        return (used > 0)
            ? InnerForward(_Buffer, _Couples, used, _Partword)
            : 0;
    }

    private int Res2Forward(EncodeBuffer _Buffer, int _PCMEnd, int[][] _Couples, bool[] _Nonzero, int _Channels,
        int[][] _Partword)
    {
        var n = _PCMEnd / 2;

        var used = false;

        // don't duplicate the code; use a working vector hack for now and
        // reshape ourselves into a single channel res1
        var work = new int[_Channels * n];
        for (var i = 0; i < _Channels; i++)
        {
            var pcm = _Couples[i];
            used = used || _Nonzero[i];

            for (int j = 0, k = i; j < n; j++, k += _Channels)
                work[k] = pcm[j];
        }

        return used
            ? InnerForward(_Buffer, new[] { work }, 1, _Partword)
            : 0;
    }

    private int InnerForward(EncodeBuffer _Buffer, int[][] _Work, int _Channels, int[][] _Partword)
    {
        var n = m_Residue.End - m_Residue.Begin;
        var partitionValues = n / m_Residue.Grouping;

        // we code the partition words for each channel, then the residual
        // words for a partition per channel until we've written all the
        // residual words for that partition word.  Then write the next
        // partition channel words
        for (var s = 0; s < m_Stages; s++)
            for (var i = 0; i < partitionValues;)
            {
                // first we encode a partition codeword for each channel 
                if (s == 0)
                    for (var j = 0; j < _Channels; j++)
                    {
                        var val = _Partword[j][i];
                        for (var k = 1; k < m_PhraseBook.Dimensions; k++)
                        {
                            val *= m_Residue.Partitions;
                            if (i + k < partitionValues)
                                val += _Partword[j][i + k];
                        }

                        if (val < m_PhraseBook.Entries)
                            _Buffer.WriteBook(m_PhraseBook, val);
                    }

                // now we encode interleaved residual values for the partitions 
                for (var k = 0; (k < m_PhraseBook.Dimensions) && (i < partitionValues); k++, i++)
                {
                    var offset = i * m_Residue.Grouping + m_Residue.Begin;

                    for (var j = 0; j < _Channels; j++)
                        if ((m_Residue.SecondStages[_Partword[j][i]] & (1 << s)) != 0)
                        {
                            var statebook = m_PartitionBooks[_Partword[j][i]][s];
                            if (statebook != null)
                                EncodePart(_Buffer, _Work[j], offset, m_Residue.Grouping, statebook);
                        }
                }
            }

        return 0;
    }

    private static void EncodePart(EncodeBuffer _Buffer, int[] _Vec, int _Offset, int _N, CodeBook _Book)
    {
        var step = _N / _Book.Dimensions;

        for (var i = 0; i < step; i++)
        {
            var entry = LocalBookBestError(_Book, _Vec, _Offset + i * _Book.Dimensions);
            _Buffer.WriteBook(_Book, entry);
        }
    }

    private static int LocalBookBestError(CodeBook _Book, int[] _Vec, int _Offset)
    {
        int i;
        int o;
        var ze = _Book.QuantValues >> 1;
        var index = 0;

        // assumes integer/centered encoder codebook maptype 1 no more than dim 8 
        Span<int> p = stackalloc int[8];
        if (_Book.Delta != 1)
            for (i = 0, o = _Book.Dimensions; i < _Book.Dimensions; i++)
            {
                var v = (_Vec[_Offset + --o] - _Book.MinVal + (_Book.Delta >> 1)) / _Book.Delta;
                var m = v < ze ? ((ze - v) << 1) - 1 : (v - ze) << 1;
                index = index * _Book.QuantValues + (m < 0 ? 0 : (m >= _Book.QuantValues ? _Book.QuantValues - 1 : m));
                p[o] = v * _Book.Delta + _Book.MinVal;
            }
        else
            for (i = 0, o = _Book.Dimensions; i < _Book.Dimensions; i++)
            {
                var v = _Vec[_Offset + --o] - _Book.MinVal;
                var m = v < ze ? ((ze - v) << 1) - 1 : (v - ze) << 1;
                index = index * _Book.QuantValues + (m < 0 ? 0 : (m >= _Book.QuantValues ? _Book.QuantValues - 1 : m));
                p[o] = v * _Book.Delta + _Book.MinVal;
            }

        if (_Book.StaticBook.LengthList[index] <= 0)
        {
            // assumes integer/centered encoder codebook maptype 1 no more than dim 8 
            var best = -1;
            var e = new int[8];
            var maxval = _Book.MinVal + _Book.Delta * (_Book.QuantValues - 1);
            for (i = 0; i < _Book.Entries; i++)
            {
                if (_Book.StaticBook.LengthList[i] > 0)
                {
                    var current = 0;
                    for (var j = 0; j < _Book.Dimensions; j++)
                    {
                        var val = e[j] - _Vec[_Offset + j];
                        current += val * val;
                    }

                    if ((best == -1) || (current < best))
                    {
                        for (var x = 0; x < e.Length; x++)
                            p[x] = e[x];

                        best = current;
                        index = i;
                    }
                }

                // assumes the value patterning created by the tools in vq
                var l = 0;
                while (e[l] >= maxval)
                    e[l++] = 0;

                if (e[l] >= 0)
                    e[l] += _Book.Delta;

                e[l] = -e[l];
            }
        }

        if (index > -1)
            for (i = 0; i < _Book.Dimensions; i++)
                _Vec[_Offset++] -= p[i];

        return index;
    }

    public int[][] Class(int[][] _Couples, bool[] _Nonzero, int _Channels)
    {
        for (var channel = 0; channel < _Channels; channel++)
        {
            if (_Nonzero[channel])
            {
                switch (m_Residue.ResidueType)
                {
                    case ResidueType.One:
                        return ResOneClass(_Couples, _Channels);
                    case ResidueType.Two:
                        return ResTwoClass(_Couples, _Channels);
                }
            }
        }
        return null;
    }

    private int[][] ResOneClass(int[][] _Couples, int _Channels)
    {
        var n = m_Residue.End - m_Residue.Begin;

        var valueCount = n / m_Residue.Grouping;

        var partword = new int[_Channels][];
        for (int c = 0; c < _Channels; c++)
            partword[c] = new int[valueCount];

        for (int i = 0; i < valueCount; i++)
        {
            int offset = i * m_Residue.Grouping + m_Residue.Begin;
            int j;
            for (j = 0; j < _Channels; j++)
            {
                int max = 0;
                int ent = 0;
                int k;
                for (k = 0; k < m_Residue.Grouping; k++)
                {
                    if (Math.Abs(_Couples[j][offset + k]) > max) max = Math.Abs(_Couples[j][offset + k]);
                    ent += Math.Abs(_Couples[j][offset + k]);
                }
                ent = (int)(ent * (100.0f / m_Residue.Grouping));

                for (k = 0; k < m_Residue.Partitions - 1; k++)
                    if (max <= m_Residue.ClassMetric1[k] &&
                       (m_Residue.ClassMetric2[k] < 0 || ent < m_Residue.ClassMetric2[k]))
                        break;

                partword[j][i] = k;
            }
        }

        return partword;
    }

    private int[][] ResTwoClass(int[][] _Couples, int _Channels)
    {
        var n = m_Residue.End - m_Residue.Begin;

        var valueCount = n / m_Residue.Grouping;

        var partword = new int[1][];
        partword[0] = new int[valueCount];

        for (int i = 0, l = m_Residue.Begin / _Channels; i < valueCount; i++)
        {
            var magMax = 0;
            var angMax = 0;
            for (var g = 0; g < m_Residue.Grouping; g += _Channels)
            {
                var abs = Math.Abs(_Couples[0][l]);
                if (abs > magMax)
                    magMax = abs;

                for (var k = 1; k < _Channels; k++)
                {
                    abs = Math.Abs(_Couples[k][l]);
                    if (abs > angMax)
                        angMax = abs;
                }

                l++;
            }

            int j;
            for (j = 0; j < m_Residue.Partitions - 1; j++)
                if ((magMax <= m_Residue.ClassMetric1[j])
                    && (angMax <= m_Residue.ClassMetric2[j]))
                    break;

            partword[0][i] = j;
        }

        return partword;
    }
}
}
