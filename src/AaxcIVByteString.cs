using CommandLineParser.Exceptions;

namespace aaxclean_cli
{
	public class AaxcIVByteString : FixedLengthByteString
	{
		public static AaxcIVByteString Parse(string hexString)
		{
			if (!TryParse(hexString, 16, out AaxcIVByteString byteString))
				throw new InvalidConversionException("IV must be 32 hex chars (16 bytes) long", "audible_iv");

			return byteString;
		}
	}
}
