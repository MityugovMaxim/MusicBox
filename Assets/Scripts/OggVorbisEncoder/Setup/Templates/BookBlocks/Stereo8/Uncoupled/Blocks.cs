using C0 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo8.Uncoupled.Chapter0;
using C1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo8.Uncoupled.Chapter1;

namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo8.Uncoupled
{
	internal class Blocks
	{
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
				new IStaticCodeBook[] { new C0.Page70(), new C0.Page71(), new C0.Page72() }
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
				new IStaticCodeBook[] { null, null, new C1.Page60() },
				new IStaticCodeBook[] { new C1.Page70(), new C1.Page71() },
				new IStaticCodeBook[] { new C1.Page80(), new C1.Page81() },
				new IStaticCodeBook[] { new C1.Page90(), new C1.Page91(), new C1.Page92() }
			};
		}
	}
}