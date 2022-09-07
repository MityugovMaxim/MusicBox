namespace OggVorbisEncoder.Setup
{
	public class AdjStereo
	{
		public AdjStereo(
			int[]   _Pre,
			int[]   _Post,
			float[] _Kilohertz,
			float[] _LowPassKilohertz
		)
		{
			Pre              = _Pre;
			Post             = _Post;
			Kilohertz        = _Kilohertz;
			LowPassKilohertz = _LowPassKilohertz;
		}

		public int[]   Pre              { get; }
		public int[]   Post             { get; }
		public float[] Kilohertz        { get; }
		public float[] LowPassKilohertz { get; }
	}
}