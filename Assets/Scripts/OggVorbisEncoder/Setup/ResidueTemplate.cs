namespace OggVorbisEncoder.Setup
{

public class ResidueTemplate : IResidueTemplate
{
    public ResidueTemplate(
        ResidueType _ResidueType,
        ResidueLimitType _LimitType,
        int _Grouping,
        ResidueEntry _Residue,
        IStaticCodeBook _BookAux,
        IStaticCodeBook _BooxAuxManaged,
        IStaticBookBlock _BooksBase,
        IStaticBookBlock _BooksBaseManaged)
    {
        ResidueType = _ResidueType;
        LimitType = _LimitType;
        Residue = _Residue;
        BookAux = _BookAux;
        BookAuxManaged = _BooxAuxManaged;
        BooksBase = _BooksBase;
        BooksBaseManaged = _BooksBaseManaged;
        Grouping = _Grouping;
    }

    public ResidueType ResidueType { get; }
    public ResidueLimitType LimitType { get; }
    public int Grouping { get; }
    public ResidueEntry Residue { get; }
    public IStaticCodeBook BookAux { get; }
    public IStaticCodeBook BookAuxManaged { get; }
    public IStaticBookBlock BooksBase { get; }
    public IStaticBookBlock BooksBaseManaged { get; }
}
}
