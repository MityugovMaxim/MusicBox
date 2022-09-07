using Blocks = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Blocks;
using CN1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.ChapterNeg1;
using C0 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter0;
using C1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter1;
using C2 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter2;
using C3 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter3;
using C4 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter4;
using C5 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter5;
using C6 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter6;
using C7 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter7;
using C8 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter8;
using C9 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter9;
using MCN1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.ManagedChapterNeg1;
using MC0 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.ManagedChapter0;
using MC1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.ManagedChapter1;

namespace OggVorbisEncoder.Setup.Templates.Residue
{
	internal static partial class Residue44
	{
		internal static readonly IStaticCodeBook  CHuffmanBookNegOneShort        = new CN1.ChapterNeg1Short();
		internal static readonly IStaticCodeBook  CHuffmanBookNegOneShortManaged = new MCN1.ManagedChapterNeg1Short();
		internal static readonly IStaticCodeBook  CHuffmanBookNegOneLong         = new CN1.ChapterNeg1Long();
		internal static readonly IStaticCodeBook  CHuffmanBookNegOneLongManaged  = new MCN1.ManagedChapterNeg1Long();
		internal static readonly IStaticBookBlock CBlockNeg1                     = new Blocks.BlockNeg1();
		internal static readonly IStaticBookBlock CBlockNeg1Managed              = new Blocks.ManagedBlockNeg1();

		internal static readonly IResidueTemplate[] CResidueNegative1 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowCoupled,
				CHuffmanBookNegOneShort,
				CHuffmanBookNegOneShortManaged,
				CBlockNeg1,
				CBlockNeg1Managed
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowCoupled,
				CHuffmanBookNegOneLong,
				CHuffmanBookNegOneLongManaged,
				CBlockNeg1,
				CBlockNeg1Managed
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook0Short        = new C0.Chapter0Short();
		internal static readonly IStaticCodeBook  CHuffmanBook0ShortManaged = new MC0.ManagedChapter0Short();
		internal static readonly IStaticCodeBook  CHuffmanBook0Long         = new C0.Chapter0Long();
		internal static readonly IStaticCodeBook  CHuffmanBook0LongManaged  = new MC0.ManagedChapter0Long();
		internal static readonly IStaticBookBlock CBlock0                   = new Blocks.Block0();
		internal static readonly IStaticBookBlock CBlock0Managed            = new Blocks.ManagedBlock0();

		internal static readonly IResidueTemplate[] CResidue0 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44LowCoupled,
				CHuffmanBook0Short,
				CHuffmanBook0ShortManaged,
				CBlock0,
				CBlock0Managed
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowCoupled,
				CHuffmanBook0Long,
				CHuffmanBook0LongManaged,
				CBlock0,
				CBlock0Managed
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook1Short        = new C1.Chapter1Short();
		internal static readonly IStaticCodeBook  CHuffmanBook1ShortManaged = new MC1.ManagedChapter1Short();
		internal static readonly IStaticCodeBook  CHuffmanBook1Long         = new C1.Chapter1Long();
		internal static readonly IStaticCodeBook  CHuffmanBook1LongManaged  = new MC1.ManagedChapter1Long();
		internal static readonly IStaticBookBlock CBlock1                   = new Blocks.Block1();
		internal static readonly IStaticBookBlock CBlock1Managed            = new Blocks.ManagedBlock1();

		internal static readonly IResidueTemplate[] CResidue1 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44LowCoupled,
				CHuffmanBook1Short,
				CHuffmanBook1ShortManaged,
				CBlock1,
				CBlock1Managed
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowCoupled,
				CHuffmanBook1Long,
				CHuffmanBook1LongManaged,
				CBlock1,
				CBlock1Managed
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook2Short = new C2.Chapter2Short();
		internal static readonly IStaticCodeBook  CHuffmanBook2Long  = new C2.Chapter2Long();
		internal static readonly IStaticBookBlock CBlock2            = new Blocks.Block2();


		internal static readonly IResidueTemplate[] CResidue2 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44MediumCoupled,
				CHuffmanBook2Short,
				CHuffmanBook2Short,
				CBlock2,
				CBlock2
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook2Long,
				CHuffmanBook2Long,
				CBlock2,
				CBlock2
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook3Short = new C3.Chapter3Short();
		internal static readonly IStaticCodeBook  CHuffmanBook3Long  = new C3.Chapter3Long();
		internal static readonly IStaticBookBlock CBlock3            = new Blocks.Block3();

		internal static readonly IResidueTemplate[] CResidue3 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44MediumCoupled,
				CHuffmanBook3Short,
				CHuffmanBook3Short,
				CBlock3,
				CBlock3
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook3Long,
				CHuffmanBook3Long,
				CBlock3,
				CBlock3
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook4Short = new C4.Chapter4Short();
		internal static readonly IStaticCodeBook  CHuffmanBook4Long  = new C4.Chapter4Long();
		internal static readonly IStaticBookBlock CBlock4            = new Blocks.Block4();

		internal static readonly IResidueTemplate[] CResidue4 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44MediumCoupled,
				CHuffmanBook4Short,
				CHuffmanBook4Short,
				CBlock4,
				CBlock4
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook4Long,
				CHuffmanBook4Long,
				CBlock4,
				CBlock4
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook5Short = new C5.Chapter5Short();
		internal static readonly IStaticCodeBook  CHuffmanBook5Long  = new C5.Chapter5Long();
		internal static readonly IStaticBookBlock CBlock5            = new Blocks.Block5();

		internal static readonly IResidueTemplate[] CResidue5 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44MediumCoupled,
				CHuffmanBook5Short,
				CHuffmanBook5Short,
				CBlock5,
				CBlock5
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumCoupled,
				CHuffmanBook5Long,
				CHuffmanBook5Long,
				CBlock5,
				CBlock5
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook6Short = new C6.Chapter6Short();
		internal static readonly IStaticCodeBook  CHuffmanBook6Long  = new C6.Chapter6Long();
		internal static readonly IStaticBookBlock CBlock6            = new Blocks.Block6();

		internal static readonly IResidueTemplate[] CResidue6 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighCoupled,
				CHuffmanBook6Short,
				CHuffmanBook6Short,
				CBlock6,
				CBlock6
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighCoupled,
				CHuffmanBook6Long,
				CHuffmanBook6Long,
				CBlock6,
				CBlock6
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook7Short = new C7.Chapter7Short();
		internal static readonly IStaticCodeBook  CHuffmanBook7Long  = new C7.Chapter7Long();
		internal static readonly IStaticBookBlock CBlock7            = new Blocks.Block7();

		internal static readonly IResidueTemplate[] CResidue7 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighCoupled,
				CHuffmanBook7Short,
				CHuffmanBook7Short,
				CBlock7,
				CBlock7
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighCoupled,
				CHuffmanBook7Long,
				CHuffmanBook7Long,
				CBlock7,
				CBlock7
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook8Short = new C8.Chapter8Short();
		internal static readonly IStaticCodeBook  CHuffmanBook8Long  = new C8.Chapter8Long();
		internal static readonly IStaticBookBlock CBlock8            = new Blocks.Block8();

		internal static readonly IResidueTemplate[] CResidue8 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighCoupled,
				CHuffmanBook8Short,
				CHuffmanBook8Short,
				CBlock8,
				CBlock8
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighCoupled,
				CHuffmanBook8Long,
				CHuffmanBook8Long,
				CBlock8,
				CBlock8
			)
		};

		internal static readonly IStaticCodeBook  CHuffmanBook9Short = new C9.Chapter9Short();
		internal static readonly IStaticCodeBook  CHuffmanBook9Long  = new C9.Chapter9Long();
		internal static readonly IStaticBookBlock CBlock9            = new Blocks.Block9();

		internal static readonly IResidueTemplate[] CResidue9 =
		{
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighCoupled,
				CHuffmanBook9Short,
				CHuffmanBook9Short,
				CBlock9,
				CBlock9
			),
			new ResidueTemplate(
				ResidueType.Two,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighCoupled,
				CHuffmanBook9Long,
				CHuffmanBook9Long,
				CBlock9,
				CBlock9
			)
		};

		public static readonly IMappingTemplate[] MapResCoupled =
		{
			new MappingTemplate(Shared.MapNominalCoupled, CResidueNegative1),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue0),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue1),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue2),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue3),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue4),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue5),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue6),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue7),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue8),
			new MappingTemplate(Shared.MapNominalCoupled, CResidue9)
		};
	}
}