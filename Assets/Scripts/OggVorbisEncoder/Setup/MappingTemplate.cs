namespace OggVorbisEncoder.Setup
{

public class MappingTemplate : IMappingTemplate
{
    public MappingTemplate(
        Mapping[] _Mapping,
        IResidueTemplate[] _ResidueTemplate)
    {
        Mapping = _Mapping;
        ResidueTemplate = _ResidueTemplate;
    }

    public Mapping[] Mapping { get; }
    public IResidueTemplate[] ResidueTemplate { get; }
}
}
