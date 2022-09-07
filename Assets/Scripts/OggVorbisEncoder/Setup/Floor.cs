using System.Linq;

namespace OggVorbisEncoder.Setup
{

public class Floor
{
    public Floor(
        int[] _PartitionClass,
        int[] _ClassDimensions,
        int[] _ClassSubs,
        int[] _ClassBook,
        int[][] _ClassSubBook,
        int _Mult,
        int[] _PostList,
        float _MaxOver,
        float _MaxUnder,
        float _MaxError,
        float _TwoFitWeight,
        float _TwoFitAtten,
        int _N)
    {
        PartitionClass = _PartitionClass;
        ClassDimensions = _ClassDimensions;
        ClassSubs = _ClassSubs;
        ClassBook = _ClassBook;
        ClassSubBook = _ClassSubBook;
        Mult = _Mult;
        PostList = _PostList;
        MaxOver = _MaxOver;
        MaxUnder = _MaxUnder;
        MaxError = _MaxError;
        TwoFitWeight = _TwoFitWeight;
        TwoFitAtten = _TwoFitAtten;
        N = _N;
    }

    /// <summary>
    ///     0 to 15
    /// </summary>
    public int[] PartitionClass { get; } // VIF_PARTS length

    /// <summary>
    ///     1 to 8
    /// </summary>
    public int[] ClassDimensions { get; }

    /// <summary>
    ///     0,1,2,3 (bits: 1&lt;&lt;n poss)
    /// </summary>
    public int[] ClassSubs { get; }

    /// <summary>
    ///     subs ^ dim entries
    /// </summary>
    public int[] ClassBook { get; }

    /// <summary>
    ///     [VIF_CLASS][subs] [VIF_CLASS][8]
    /// </summary>
    public int[][] ClassSubBook { get; }

    /// <summary>
    ///     1 2 3 or 4
    /// </summary>
    public int Mult { get; }

    /// <summary>
    ///     first two implicit
    /// </summary>
    public int[] PostList { get; }

    /* encode side analysis parameters */
    public float MaxOver { get; }
    public float MaxUnder { get; }
    public float MaxError { get; }
    public float TwoFitWeight { get; }
    public float TwoFitAtten { get; }
    public int N { get; set; }

    public Floor Clone() =>
        new Floor(
            PartitionClass.ToArray(),
            ClassDimensions.ToArray(),
            ClassSubs.ToArray(),
            ClassBook.ToArray(),
            ClassSubBook.Select(_S => _S.ToArray()).ToArray(),
            Mult,
            PostList.ToArray(),
            MaxOver,
            MaxUnder,
            MaxError,
            TwoFitWeight,
            TwoFitAtten,
            N);
}
}
