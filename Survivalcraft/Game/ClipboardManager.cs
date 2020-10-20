using Engine;
using System;
using System.Windows.Forms;

namespace Game
{
	public static class ClipboardManager
	{
		public static string ClipboardString
		{
			get
			{
				return Clipboard.GetText();
			}
			set
			{
				Clipboard.SetText(value);
			}
		}
	}
}
