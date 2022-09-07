using U0 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16.Uncoupled.Chapter0;
using U1 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16.Uncoupled.Chapter1;
using U2 = OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16.Uncoupled.Chapter2;

namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16.Uncoupled
{
	internal class Blocks
	{
		public class Block0 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new U0.Page10() },
				new IStaticCodeBook[] { null, null, new U0.Page20() },
				new IStaticCodeBook[] { null, null, new U0.Page30() },
				new IStaticCodeBook[] { null, null, new U0.Page40() },
				new IStaticCodeBook[] { null, null, new U0.Page50() },
				new IStaticCodeBook[] { new U0.Page60(), new U0.Page61() },
				new IStaticCodeBook[] { new U0.Page70(), new U0.Page71(), new U0.Page72() }
			};
		}

		public class Block1 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new U1.Page10() },
				new IStaticCodeBook[] { null, null, new U1.Page20() },
				new IStaticCodeBook[] { null, null, new U1.Page30() },
				new IStaticCodeBook[] { null, null, new U1.Page40() },
				new IStaticCodeBook[] { null, null, new U1.Page50() },
				new IStaticCodeBook[] { null, null, new U1.Page60() },
				new IStaticCodeBook[] { new U1.Page70(), new U1.Page71() },
				new IStaticCodeBook[] { new U1.Page80(), new U1.Page81() },
				new IStaticCodeBook[] { new U1.Page90(), new U1.Page91(), new U1.Page92() }
			};
		}

		public class Block2 : IStaticBookBlock
		{
			public IStaticCodeBook[][] Books { get; } =
			{
				new IStaticCodeBook[] { null },
				new IStaticCodeBook[] { null, null, new U2.Page10() },
				new IStaticCodeBook[] { null, null, new U2.Page20() },
				new IStaticCodeBook[] { null, null, new U2.Page30() },
				new IStaticCodeBook[] { null, null, new U2.Page40() },
				new IStaticCodeBook[] { new U2.Page50(), new U2.Page51() },
				new IStaticCodeBook[] { new U2.Page60(), new U2.Page61() },
				new IStaticCodeBook[] { new U2.Page70(), new U2.Page71() },
				new IStaticCodeBook[] { new U2.Page80(), new U2.Page81() },
				new IStaticCodeBook[] { new U2.Page90(), new U2.Page91(), new U2.Page92() }
			};
		}
	}
}