namespace OggVorbisEncoder.Setup
{

public struct Mode
{
    public int BlockFlag;
    public int WindowType;
    public int TransformType;
    public int Mapping;

    public Mode(
        int _BlockFlag,
        int _WindowType,
        int _TransformType,
        int _Mapping)
    {
        BlockFlag = _BlockFlag;
        WindowType = _WindowType;
        TransformType = _TransformType;
        Mapping = _Mapping;
    }
}
}
