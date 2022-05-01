using CommandLineParser.Exceptions;

namespace aaxclean_cli
{
	public class ActivationByteString : FixedLengthByteString
	{
		public static ActivationByteString Parse(string hexString)
		{
			if (!TryParse(hexString, 4, out ActivationByteString byteString))
				throw new InvalidConversionException("Activation Bytes must be 8 hex chars (4 bytes) long", "activation_bytes");

			return byteString;
		}
	}
}
