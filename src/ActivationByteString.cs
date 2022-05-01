using CommandLineParser.Exceptions;

namespace aaxclean_cli
{
	public class ActivationByteString : FixedLengthByteString
	{
		public static ActivationByteString Parse(string hexString, System.Globalization.CultureInfo cultureInfo)
		{
			if (!TryParse(hexString, 4, out byte[] bytes))
				throw new InvalidConversionException("Activation Bytes must be 8 hex chars (4 bytes) long", "activation_bytes");

			return new ActivationByteString { Bytes = bytes };
		}
	}
}
