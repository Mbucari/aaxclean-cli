namespace aaxclean_cli
{
	public abstract class FixedLengthByteString
	{
		public byte[] Bytes { get; protected set; }

		public static bool TryParse<T>(string hexString, int expectedLength, out T byteString ) where T : FixedLengthByteString, new()
		{
			hexString = hexString.Trim();
			if (hexString.Length != expectedLength * 2)
			{
				byteString = null;
				return false;
			}

			var bytes = new byte[expectedLength];

			for (int i = 0; i < bytes.Length; i++)
			{
				if (byte.TryParse(hexString.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
				{
					bytes[i] = b;
				}
				else
				{
					byteString = null;
					return false;
				}
			}

			byteString = new T { Bytes = bytes };
			return true;
		}
	}
}
