namespace OggVorbisEncoder.Setup
{

public class ResidueEntry
{
    public ResidueEntry(
        int _Begin,
        int _End,
        int _Grouping,
        int _Partitions,
        int _PartitionValues,
        int _GroupBook,
        int[] _SecondStages,
        int[] _BookList,
        int[] _ClassMetric1,
        int[] _ClassMetric2,
        ResidueType _ResidueType)
    {
        Begin = _Begin;
        End = _End;
        Grouping = _Grouping;
        Partitions = _Partitions;
        PartitionValues = _PartitionValues;
        GroupBook = _GroupBook;
        SecondStages = _SecondStages.ToFixedLength(64);
        BookList = _BookList.ToFixedLength(512);
        ClassMetric1 = _ClassMetric1.ToFixedLength(64);
        ClassMetric2 = _ClassMetric2.ToFixedLength(64);
        ResidueType = _ResidueType;
    }

    public int Begin { get; }
    public int End { get; set; }
    public int Partitions { get; }
    public int PartitionValues { get; }
    public int GroupBook { get; set; }
    public int[] SecondStages { get; }
    public int[] BookList { get; }
    public int[] ClassMetric1 { get; }
    public int[] ClassMetric2 { get; }

    public ResidueType ResidueType { get; }
    public int Grouping { get; }

    public ResidueEntry Clone(ResidueType _ResidueTypeOverride, int _GroupingOverride)
        => new ResidueEntry(
            Begin,
            End,
            _GroupingOverride,
            Partitions,
            PartitionValues,
            GroupBook,
            SecondStages,
            BookList,
            ClassMetric1,
            ClassMetric2,
            _ResidueTypeOverride);
}
}
