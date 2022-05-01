using CommandLineParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaxclean_cli
{
	public class ActivationByteString : FixedLengthByteString
	{
		public static ActivationByteString Parse(string hexString, System.Globalization.CultureInfo cultureInfo)
		{
			if (!TryParse(hexString, 4, out byte[] bytes))
				throw new InvalidConversionException("Activation Bytes must be 8 hex chars (4 bytes) long", "activation_bytes");

			return new ActivationByteString { ByteString = hexString, Bytes = bytes };
		}
	}
}
