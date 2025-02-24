using System;
using System.Globalization;

namespace aaxclean_cli;

public class FixedLengthByteString
{
	public byte[] Bytes { get; init; }

	public FixedLengthByteString(string hexString, int byteCount, string optionName)
	{
		if (hexString == null) throw new ArgumentNullException(nameof(hexString));

		if (!TryParse(hexString, byteCount, out var byteString))
			throw new InvalidCastException($"{optionName} must be {byteCount * 2} hex chars ({byteCount} bytes) long");

		Bytes = byteString;
	}

	private static bool TryParse(string hexString, int expectedLength, out byte[] byteString)
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
			if (byte.TryParse(hexString.Substring(2 * i, 2), NumberStyles.HexNumber, null, out byte b))
			{
				bytes[i] = b;
			}
			else
			{
				byteString = null;
				return false;
			}
		}

		byteString = bytes;
		return true;
	}
}
