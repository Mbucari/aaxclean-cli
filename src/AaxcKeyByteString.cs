using CommandLineParser.Exceptions;

namespace aaxclean_cli
{
	public class AaxcKeyByteString : FixedLengthByteString
	{
		public static AaxcKeyByteString Parse(string hexString)
		{
			if (!TryParse(hexString, 16, out AaxcKeyByteString byteString))
				throw new InvalidConversionException("Key must be 32 hex chars (16 bytes) long", "audible_key");

			return byteString;
		}
	}	
}
