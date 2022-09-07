using S8 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo8;

namespace OggVorbisEncoder.Setup.Templates.Residue
{
	internal static class Residue8
	{
		private static readonly IStaticCodeBook  m_CHuffmanBook0Single = new S8.Coupled.Chapter0.Chapter0Single();
		private static readonly IStaticBookBlock m_CBlock0             = new S8.Coupled.Blocks.Block0();

		internal static readonly IResidueTemplate[] CResidue0 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				m_CHuffmanBook0Single,
				m_CHuffmanBook0Single,
				m_CBlock0,
				m_CBlock0
			),
		};

		private static readonly IStaticCodeBook  m_CHuffmanBook1Single = new S8.Coupled.Chapter1.Chapter1Single();
		private static readonly IStaticBookBlock m_CBlock1             = new S8.Coupled.Blocks.Block1();

		internal static readonly IResidueTemplate[] CResidue1 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				m_CHuffmanBook1Single,
				m_CHuffmanBook1Single,
				m_CBlock1,
				m_CBlock1
			),
		};

		internal static readonly IMappingTemplate[] MapResCoupled =
		{
			new MappingTemplate(Shared.MapNominalCoupled, CResidue0),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue1)
		};

		private static readonly IStaticCodeBook  m_UHuffmanBook0Single = new S8.Uncoupled.Chapter0.Chapter0Single();
		private static readonly IStaticBookBlock m_UBlock0             = new S8.Uncoupled.Blocks.Block0();

		internal static readonly IResidueTemplate[] UResidue0 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumUncoupled,
				m_UHuffmanBook0Single,
				m_UHuffmanBook0Single,
				m_UBlock0,
				m_UBlock0
			)
		};

		private static readonly IStaticCodeBook  m_UHuffmanBook1Single = new S8.Uncoupled.Chapter1.Chapter1Single();
		private static readonly IStaticBookBlock m_UBlock1             = new S8.Uncoupled.Blocks.Block1();

		internal static readonly IResidueTemplate[] UResidue1 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumUncoupled,
				m_UHuffmanBook1Single,
				m_UHuffmanBook1Single,
				m_UBlock1,
				m_UBlock1
			)
		};

		internal static readonly IMappingTemplate[] MapResUncoupled =
		{
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue0),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue1)
		};
	}
}