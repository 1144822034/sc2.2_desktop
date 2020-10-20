using System;

namespace Hjg.Pngcs
{
	internal class PngjOutputException : PngjException
	{
		private const long serialVersionUID = 1L;

		public PngjOutputException(string message, Exception cause)
			: base(message, cause)
		{
		}

		public PngjOutputException(string message)
			: base(message)
		{
		}

		public PngjOutputException(Exception cause)
			: base(cause)
		{
		}
	}
}
