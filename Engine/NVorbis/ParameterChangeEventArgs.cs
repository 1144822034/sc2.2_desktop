using System;

namespace NVorbis
{
	internal class ParameterChangeEventArgs : EventArgs
	{
		public DataPacket FirstPacket
		{
			get;
			private set;
		}

		public ParameterChangeEventArgs(DataPacket firstPacket)
		{
			FirstPacket = firstPacket;
		}
	}
}
