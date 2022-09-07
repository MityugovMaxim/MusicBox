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

namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled
{
	public class Blocks
	{
		public class BlockNeg1 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new CN1.Page10() },
				new IStaticCodeBook[] { null, null, new CN1.Page20() },
				new IStaticCodeBook[] { null, null, new CN1.Page30() },
				new IStaticCodeBook[] { null, null, new CN1.Page40() },
				new IStaticCodeBook[] { null, null, new CN1.Page50() },
				new IStaticCodeBook[] { new CN1.Page60(), new CN1.Page61() },
				new IStaticCodeBook[] { new CN1.Page70(), new CN1.Page71() },
				new IStaticCodeBook[] { new CN1.Page80(), new CN1.Page81(), new CN1.Page82() }
			};
		}

		public class Block0 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C0.Page10() },
				new IStaticCodeBook[] { null, null, new C0.Page20() },
				new IStaticCodeBook[] { null, null, new C0.Page30() },
				new IStaticCodeBook[] { null, null, new C0.Page40() },
				new IStaticCodeBook[] { null, null, new C0.Page50() },
				new IStaticCodeBook[] { new C0.Page60(), new C0.Page61() },
				new IStaticCodeBook[] { new C0.Page70(), new C0.Page71() },
				new IStaticCodeBook[] { new C0.Page80(), new C0.Page81(), new C0.Page82() }
			};
		}

		public class Block1 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C1.Page10() },
				new IStaticCodeBook[] { null, null, new C1.Page20() },
				new IStaticCodeBook[] { null, null, new C1.Page30() },
				new IStaticCodeBook[] { null, null, new C1.Page40() },
				new IStaticCodeBook[] { null, null, new C1.Page50() },
				new IStaticCodeBook[] { new C1.Page60(), new C1.Page61() },
				new IStaticCodeBook[] { new C1.Page70(), new C1.Page71() },
				new IStaticCodeBook[] { new C1.Page80(), new C1.Page81(), new C1.Page82() }
			};
		}

		public class Block2 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C2.Page10() },
				new IStaticCodeBook[] { null, null, new C2.Page20() },
				new IStaticCodeBook[] { null, null, new C2.Page30() },
				new IStaticCodeBook[] { null, null, new C2.Page40() },
				new IStaticCodeBook[] { null, null, new C2.Page50() },
				new IStaticCodeBook[] { null, null, new C2.Page60() },
				new IStaticCodeBook[] { new C2.Page70(), new C2.Page71() },
				new IStaticCodeBook[] { new C2.Page80(), new C2.Page81() },
				new IStaticCodeBook[] { new C2.Page90(), new C2.Page91(), new C2.Page92() }
			};
		}

		public class Block3 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C3.Page10() },
				new IStaticCodeBook[] { null, null, new C3.Page20() },
				new IStaticCodeBook[] { null, null, new C3.Page30() },
				new IStaticCodeBook[] { null, null, new C3.Page40() },
				new IStaticCodeBook[] { null, null, new C3.Page50() },
				new IStaticCodeBook[] { null, null, new C3.Page60() },
				new IStaticCodeBook[] { new C3.Page70(), new C3.Page71() },
				new IStaticCodeBook[] { new C3.Page80(), new C3.Page81() },
				new IStaticCodeBook[] { new C3.Page90(), new C3.Page91(), new C3.Page92() }
			};
		}

		public class Block4 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C4.Page10() },
				new IStaticCodeBook[] { null, null, new C4.Page20() },
				new IStaticCodeBook[] { null, null, new C4.Page30() },
				new IStaticCodeBook[] { null, null, new C4.Page40() },
				new IStaticCodeBook[] { null, null, new C4.Page50() },
				new IStaticCodeBook[] { null, null, new C4.Page60() },
				new IStaticCodeBook[] { new C4.Page70(), new C4.Page71() },
				new IStaticCodeBook[] { new C4.Page80(), new C4.Page81() },
				new IStaticCodeBook[] { new C4.Page90(), new C4.Page91(), new C4.Page92() }
			};
		}

		public class Block5 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C5.Page10() },
				new IStaticCodeBook[] { null, null, new C5.Page20() },
				new IStaticCodeBook[] { null, null, new C5.Page30() },
				new IStaticCodeBook[] { null, null, new C5.Page40() },
				new IStaticCodeBook[] { null, null, new C5.Page50() },
				new IStaticCodeBook[] { null, null, new C5.Page60() },
				new IStaticCodeBook[] { new C5.Page70(), new C5.Page71() },
				new IStaticCodeBook[] { new C5.Page80(), new C5.Page81() },
				new IStaticCodeBook[] { new C5.Page90(), new C5.Page91(), new C5.Page92() }
			};
		}

		public class Block6 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C6.Page10() },
				new IStaticCodeBook[] { null, null, new C6.Page20() },
				new IStaticCodeBook[] { null, null, new C6.Page30() },
				new IStaticCodeBook[] { null, null, new C6.Page40() },
				new IStaticCodeBook[] { new C6.Page50(), new C6.Page51() },
				new IStaticCodeBook[] { new C6.Page60(), new C6.Page61() },
				new IStaticCodeBook[] { new C6.Page70(), new C6.Page71() },
				new IStaticCodeBook[] { new C6.Page80(), new C6.Page81() },
				new IStaticCodeBook[] { new C6.Page90(), new C6.Page91(), new C6.Page92() }
			};
		}

		public class Block7 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C7.Page10() },
				new IStaticCodeBook[] { null, null, new C7.Page20() },
				new IStaticCodeBook[] { null, null, new C7.Page30() },
				new IStaticCodeBook[] { null, null, new C7.Page40() },
				new IStaticCodeBook[] { new C7.Page50(), new C7.Page51() },
				new IStaticCodeBook[] { new C7.Page60(), new C7.Page61() },
				new IStaticCodeBook[] { new C7.Page70(), new C7.Page71() },
				new IStaticCodeBook[] { new C7.Page80(), new C7.Page81() },
				new IStaticCodeBook[] { new C7.Page90(), new C7.Page91(), new C7.Page92() }
			};
		}

		public class Block8 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C8.Page10() },
				new IStaticCodeBook[] { null, null, new C8.Page20() },
				new IStaticCodeBook[] { null, null, new C8.Page30() },
				new IStaticCodeBook[] { null, null, new C8.Page40() },
				new IStaticCodeBook[] { new C8.Page50(), new C8.Page51() },
				new IStaticCodeBook[] { new C8.Page60(), new C8.Page61() },
				new IStaticCodeBook[] { new C8.Page70(), new C8.Page71() },
				new IStaticCodeBook[] { new C8.Page80(), new C8.Page81() },
				new IStaticCodeBook[] { new C8.Page90(), new C8.Page91(), new C8.Page92() }
			};
		}

		public class Block9 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new C9.Page10() },
				new IStaticCodeBook[] { null, null, new C9.Page20() },
				new IStaticCodeBook[] { null, null, new C9.Page30() },
				new IStaticCodeBook[] { null, null, new C9.Page40() },
				new IStaticCodeBook[] { new C9.Page50(), new C9.Page51() },
				new IStaticCodeBook[] { new C9.Page60(), new C9.Page61() },
				new IStaticCodeBook[] { new C9.Page70(), new C9.Page71() },
				new IStaticCodeBook[] { new C9.Page80(), new C9.Page81() },
				new IStaticCodeBook[] { new C9.Page90(), new C9.Page91(), new C9.Page92() }
			};
		}


		public class ManagedBlockNeg1 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new MCN1.Page10() },
				new IStaticCodeBook[] { null, null, new MCN1.Page20() },
				new IStaticCodeBook[] { null, null, new MCN1.Page30() },
				new IStaticCodeBook[] { null, null, new MCN1.Page40() },
				new IStaticCodeBook[] { null, null, new MCN1.Page50() },
				new IStaticCodeBook[] { new MCN1.Page60(), new MCN1.Page61() },
				new IStaticCodeBook[] { new MCN1.Page70(), new MCN1.Page71() },
				new IStaticCodeBook[] { new MCN1.Page80(), new MCN1.Page81(), new CN1.Page82() }
			};
		}

		public class ManagedBlock0 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new MC0.Page10() },
				new IStaticCodeBook[] { null, null, new MC0.Page20() },
				new IStaticCodeBook[] { null, null, new MC0.Page30() },
				new IStaticCodeBook[] { null, null, new MC0.Page40() },
				new IStaticCodeBook[] { null, null, new MC0.Page50() },
				new IStaticCodeBook[] { new MC0.Page60(), new MC0.Page61() },
				new IStaticCodeBook[] { new MC0.Page70(), new MC0.Page71() },
				new IStaticCodeBook[] { new MC0.Page80(), new MC0.Page81(), new C0.Page82() }
			};
		}

		public class ManagedBlock1 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new MC1.Page10() },
				new IStaticCodeBook[] { null, null, new MC1.Page20() },
				new IStaticCodeBook[] { null, null, new MC1.Page30() },
				new IStaticCodeBook[] { null, null, new MC1.Page40() },
				new IStaticCodeBook[] { null, null, new MC1.Page50() },
				new IStaticCodeBook[] { new MC1.Page60(), new MC1.Page61() },
				new IStaticCodeBook[] { new MC1.Page70(), new MC1.Page71() },
				new IStaticCodeBook[] { new MC1.Page80(), new MC1.Page81(), new C1.Page82() }
			};
		}
	}
}