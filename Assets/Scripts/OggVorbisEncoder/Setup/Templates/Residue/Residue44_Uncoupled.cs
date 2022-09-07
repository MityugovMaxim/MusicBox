using Blocks = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Blocks;
using CN1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.ChapterNeg1;
using C0 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter0;
using C1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter1;
using C2 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter2;
using C3 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter3;
using C4 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter4;
using C5 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter5;
using C6 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter6;
using C7 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter7;
using C8 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter8;
using C9 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter9;

namespace OggVorbisEncoder.Setup.Templates.Residue
{
	internal static partial class Residue44
	{
		internal static readonly IStaticCodeBook  UHuffmanBookNegOneShort = new CN1.ChapterNeg1Short();
		internal static readonly IStaticCodeBook  UHuffmanBookNegOneLong  = new CN1.ChapterNeg1Long();
		internal static readonly IStaticBookBlock UBlockNeg1              = new Blocks.BlockNeg1();

		internal static readonly IResidueTemplate[] UResidueNegative1 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowUncoupled,
				UHuffmanBookNegOneShort,
				UHuffmanBookNegOneShort,
				UBlockNeg1,
				UBlockNeg1
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowUncoupled,
				UHuffmanBookNegOneLong,
				UHuffmanBookNegOneLong,
				UBlockNeg1,
				UBlockNeg1
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook0Short = new C0.Chapter0Short();
		internal static readonly IStaticCodeBook  UHuffmanBook0Long  = new C0.Chapter0Long();
		internal static readonly IStaticBookBlock UBlock0            = new Blocks.Block0();

		internal static readonly IResidueTemplate[] UResidue0 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44LowUncoupled,
				UHuffmanBook0Short,
				UHuffmanBook0Short,
				UBlock0,
				UBlock0
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowUncoupled,
				UHuffmanBook0Long,
				UHuffmanBook0Long,
				UBlock0,
				UBlock0
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook1Short = new C1.Chapter1Short();
		internal static readonly IStaticCodeBook  UHuffmanBook1Long  = new C1.Chapter1Long();
		internal static readonly IStaticBookBlock UBlock1            = new Blocks.Block1();

		internal static readonly IResidueTemplate[] UResidue1 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44LowUncoupled,
				UHuffmanBook1Short,
				UHuffmanBook1Short,
				UBlock1,
				UBlock1
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44LowUncoupled,
				UHuffmanBook1Long,
				UHuffmanBook1Long,
				UBlock1,
				UBlock1
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook2Short = new C2.Chapter2Short();
		internal static readonly IStaticCodeBook  UHuffmanBook2Long  = new C2.Chapter2Long();
		internal static readonly IStaticBookBlock UBlock2            = new Blocks.Block2();

		internal static readonly IResidueTemplate[] UResidue2 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook2Short,
				UHuffmanBook2Short,
				UBlock2,
				UBlock2
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook2Long,
				UHuffmanBook2Long,
				UBlock2,
				UBlock2
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook3Short = new C3.Chapter3Short();
		internal static readonly IStaticCodeBook  UHuffmanBook3Long  = new C3.Chapter3Long();
		internal static readonly IStaticBookBlock UBlock3            = new Blocks.Block3();

		internal static readonly IResidueTemplate[] UResidue3 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				16,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook3Short,
				UHuffmanBook3Short,
				UBlock3,
				UBlock3
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				32,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook3Long,
				UHuffmanBook3Long,
				UBlock3,
				UBlock3
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook4Short = new C4.Chapter4Short();
		internal static readonly IStaticCodeBook  UHuffmanBook4Long  = new C4.Chapter4Long();
		internal static readonly IStaticBookBlock UBlock4            = new Blocks.Block4();

		internal static readonly IResidueTemplate[] UResidue4 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				16,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook4Short,
				UHuffmanBook4Short,
				UBlock4,
				UBlock4
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				32,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook4Long,
				UHuffmanBook4Long,
				UBlock4,
				UBlock4
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook5Short = new C5.Chapter5Short();
		internal static readonly IStaticCodeBook  UHuffmanBook5Long  = new C5.Chapter5Long();
		internal static readonly IStaticBookBlock UBlock5            = new Blocks.Block5();

		internal static readonly IResidueTemplate[] UResidue5 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				16,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook5Short,
				UHuffmanBook5Short,
				UBlock5,
				UBlock5
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				32,
				Shared.Residue44MediumUncoupled,
				UHuffmanBook5Long,
				UHuffmanBook5Long,
				UBlock5,
				UBlock5
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook6Short = new C6.Chapter6Short();
		internal static readonly IStaticCodeBook  UHuffmanBook6Long  = new C6.Chapter6Long();
		internal static readonly IStaticBookBlock UBlock6            = new Blocks.Block6();

		internal static readonly IResidueTemplate[] UResidue6 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				16,
				Shared.Residue44HighUncoupled,
				UHuffmanBook6Short,
				UHuffmanBook6Short,
				UBlock6,
				UBlock6
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass,
				32,
				Shared.Residue44HighUncoupled,
				UHuffmanBook6Long,
				UHuffmanBook6Long,
				UBlock6,
				UBlock6
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook7Short = new C7.Chapter7Short();
		internal static readonly IStaticCodeBook  UHuffmanBook7Long  = new C7.Chapter7Long();
		internal static readonly IStaticBookBlock UBlock7            = new Blocks.Block7();

		internal static readonly IResidueTemplate[] UResidue7 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighUncoupled,
				UHuffmanBook7Short,
				UHuffmanBook7Short,
				UBlock7,
				UBlock7
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighUncoupled,
				UHuffmanBook7Long,
				UHuffmanBook7Long,
				UBlock7,
				UBlock7
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook8Short = new C8.Chapter8Short();
		internal static readonly IStaticCodeBook  UHuffmanBook8Long  = new C8.Chapter8Long();
		internal static readonly IStaticBookBlock UBlock8            = new Blocks.Block8();

		internal static readonly IResidueTemplate[] UResidue8 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighUncoupled,
				UHuffmanBook8Short,
				UHuffmanBook8Short,
				UBlock8,
				UBlock8
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighUncoupled,
				UHuffmanBook8Long,
				UHuffmanBook8Long,
				UBlock8,
				UBlock8
			)
		};

		internal static readonly IStaticCodeBook  UHuffmanBook9Short = new C9.Chapter9Short();
		internal static readonly IStaticCodeBook  UHuffmanBook9Long  = new C9.Chapter9Long();
		internal static readonly IStaticBookBlock UBlock9            = new Blocks.Block9();

		internal static readonly IResidueTemplate[] UResidue9 =
		{
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 16,
				Shared.Residue44HighUncoupled,
				UHuffmanBook9Short,
				UHuffmanBook9Short,
				UBlock9,
				UBlock9
			),
			new ResidueTemplate(
				ResidueType.One,
				ResidueLimitType.LowPass, 32,
				Shared.Residue44HighUncoupled,
				UHuffmanBook9Long,
				UHuffmanBook9Long,
				UBlock9,
				UBlock9
			)
		};

		public static readonly IMappingTemplate[] MapResUncoupled =
		{
			new MappingTemplate(Shared.MapNominalUncoupled, UResidueNegative1),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue0),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue1),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue2),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue3),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue4),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue5),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue6),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue7),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue8),
			new MappingTemplate(Shared.MapNominalUncoupled, UResidue9)
		};
	}
}