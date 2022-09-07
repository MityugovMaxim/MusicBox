using S16 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16;

namespace OggVorbisEncoder.Setup.Templates.Residue
{
	internal static class Residue16
	{
		internal static readonly IStaticCodeBook  CHuffmanBook0Single = new S16.Coupled.Chapter0.Chapter0Single();
		internal static readonly IStaticBookBlock CBlock0             = new S16.Coupled.Blocks.Block0();

		internal static readonly IResidueTemplate[] CResidue1 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook0Single,
				CHuffmanBook0Single,
				CBlock0,
				CBlock0
			),
		};

		internal static readonly IStaticCodeBook  CHuffmanBook1Short = new S16.Coupled.Chapter1.Chapter1Short();
		internal static readonly IStaticCodeBook  CHuffmanBook1Long  = new S16.Coupled.Chapter1.Chapter1Long();
		internal static readonly IStaticBookBlock CBlock1            = new S16.Coupled.Blocks.Block1();

		internal static readonly IResidueTemplate[] CResidue2 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook1Short,
				CHuffmanBook1Short,
				CBlock1,
				CBlock1
			),

			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook1Long,
				CHuffmanBook1Long,
				CBlock1,
				CBlock1
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook2Short = new S16.Coupled.Chapter2.Chapter2Short();
		internal static readonly IStaticCodeBook  CHuffmanBook2Long  = new S16.Coupled.Chapter2.Chapter2Long();
		internal static readonly IStaticBookBlock CBlock2            = new S16.Coupled.Blocks.Block2();

		internal static readonly IResidueTemplate[] CResidue3 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighCoupled,
				CHuffmanBook2Short,
				CHuffmanBook2Short,
				CBlock2,
				CBlock2
			),

			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighCoupled,
				CHuffmanBook2Long,
				CHuffmanBook2Long,
				CBlock2,
				CBlock2
			)
		};

		internal static readonly IMappingTemplate[] MapResStereo =
		{
			new MappingTemplate(Shared.MapNominalCoupled, CResidue1),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue2),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue3),
		};

		internal static readonly IStaticCodeBook  UHuffmanBook0Single = new S16.Uncoupled.Chapter0.Chapter0Single();
		internal static readonly IStaticBookBlock UBlock0             = new S16.Uncoupled.Blocks.Block0();

		internal static readonly IResidueTemplate[] UResidue0 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowCoupled,
				UHuffmanBook0Single,
				UHuffmanBook0Single,
				UBlock0,
				UBlock0
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook1Short = new S16.Uncoupled.Chapter1.Chapter1Short();
		internal static readonly IStaticCodeBook  UHuffmanBook1Long  = new S16.Uncoupled.Chapter1.Chapter1Long();
		internal static readonly IStaticBookBlock UBlock1            = new S16.Uncoupled.Blocks.Block1();

		internal static readonly IResidueTemplate[] UResidue1 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				UHuffmanBook1Short,
				UHuffmanBook1Short,
				UBlock1,
				UBlock1
			),


			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				UHuffmanBook1Long,
				UHuffmanBook1Long,
				UBlock1,
				UBlock1
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook2Short = new S16.Uncoupled.Chapter2.Chapter2Short();
		internal static readonly IStaticCodeBook  UHuffmanBook2Long  = new S16.Uncoupled.Chapter2.Chapter2Long();
		internal static readonly IStaticBookBlock UBlock2            = new S16.Uncoupled.Blocks.Block2();

		internal static readonly IResidueTemplate[] UResidue2 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighUncoupled,
				UHuffmanBook2Short,
				UHuffmanBook2Short,
				UBlock2,
				UBlock2
			),

			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighUncoupled,
				UHuffmanBook2Long,
				UHuffmanBook2Long,
				UBlock2,
				UBlock2
			)
		};

		internal static readonly IMappingTemplate[] MapResUncoupled =
		{
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue0),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue1),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue2)
		};
	}
}