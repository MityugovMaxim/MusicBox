namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16.Coupled.Chapter2
{
	public class Page91 : IStaticCodeBook
	{
		public int Dimensions { get; } = 2;

		public byte[] LengthList { get; } =
		{
			1, 4, 4, 7, 7, 7, 7, 7, 7, 8, 8, 10, 9, 11, 10, 13,
			11, 14, 13, 6, 6, 6, 8, 8, 8, 8, 8, 7, 9, 8, 11, 9,
			13, 11, 14, 12, 14, 13, 5, 6, 6, 8, 8, 8, 8, 8, 8, 9,
			9, 11, 11, 13, 11, 14, 13, 15, 15, 17, 8, 8, 8, 8, 9, 9,
			9, 8, 11, 9, 12, 10, 13, 11, 14, 12, 14, 13, 17, 8, 8, 8,
			8, 9, 9, 9, 9, 10, 10, 11, 11, 13, 13, 13, 14, 16, 15, 17,
			12, 12, 8, 8, 9, 9, 10, 10, 11, 11, 12, 11, 13, 12, 13, 12,
			14, 13, 16, 12, 12, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13,
			13, 14, 14, 15, 15, 17, 17, 17, 9, 9, 9, 9, 11, 11, 12, 12,
			12, 13, 13, 13, 16, 14, 14, 14, 17, 17, 17, 9, 8, 9, 8, 11,
			10, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 17, 12, 12,
			10, 10, 11, 12, 12, 13, 13, 14, 13, 15, 15, 14, 16, 15, 17, 17,
			17, 12, 12, 10, 8, 12, 9, 13, 12, 14, 14, 15, 14, 15, 16, 16,
			16, 17, 17, 17, 17, 17, 11, 11, 12, 12, 14, 14, 14, 16, 15, 16,
			15, 16, 15, 17, 17, 17, 17, 17, 17, 11, 9, 12, 10, 13, 11, 15,
			14, 16, 16, 17, 16, 16, 15, 17, 17, 17, 17, 17, 15, 15, 12, 12,
			14, 14, 15, 16, 16, 15, 16, 16, 17, 17, 17, 17, 17, 17, 17, 14,
			14, 12, 10, 14, 11, 15, 12, 17, 16, 15, 16, 17, 16, 17, 17, 17,
			17, 17, 17, 17, 13, 13, 14, 14, 14, 16, 17, 17, 16, 17, 17, 17,
			17, 17, 17, 17, 17, 17, 17, 13, 9, 13, 12, 15, 13, 16, 16, 17,
			17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 15, 17, 14, 14, 15, 16,
			16, 17, 16, 17, 16, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 14,
			13, 15, 16, 16, 17, 16, 17, 17, 17
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -518287360;
		public int             QuantDelta     { get; } = 1622704128;
		public int             Quant          { get; } = 5;
		public int             QuantSequenceP { get; } = 0;

		public int[] QuantList { get; } =
		{
			9,
			8,
			10,
			7,
			11,
			6,
			12,
			5,
			13,
			4,
			14,
			3,
			15,
			2,
			16,
			1,
			17,
			0,
			18
		};
	}
}