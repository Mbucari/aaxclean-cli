using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaxclean_cli
{
	public abstract class FixedLengthByteString
	{
		public byte[] Bytes { get; protected set; }

		public static bool TryParse(string hexString, int expectedLength, out byte[] bytes )
		{

			hexString = hexString.Trim();
			if (hexString.Length != expectedLength * 2)
			{
				bytes = null;
				return false;
			}
			bytes = new byte[expectedLength];
			for (int i = 0; i < bytes.Length; i++)
			{
				if (byte.TryParse(hexString.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
				{
					bytes[i] = b;
				}
				else
				{
					bytes = null;
					return false;
				}
			}
			return true;
		}
	}
}
