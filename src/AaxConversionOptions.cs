using AAXClean;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace aaxclean_cli
{

	[DistinctGroupsCertification("url,user_agent,cookie", "file")]
	[ArgumentGroupCertification("file,url", EArgumentGroupCondition.ExactlyOneUsed)]
	[DistinctGroupsCertification("activation_bytes", "audible_key,audible_iv")]
	[ArgumentGroupCertification("activation_bytes", EArgumentGroupCondition.AllOrNoneUsed)]
	[ArgumentGroupCertification("audible_key,audible_iv", EArgumentGroupCondition.AllOrNoneUsed)]
	[ArgumentGroupCertification("activation_bytes,audible_key", EArgumentGroupCondition.AtLeastOneUsed)]
	public class AaxConversionOptions
	{
		[FileArgument('f', "file", AllowMultiple = false, Description = "Aax(c) input from file")]
		public FileInfo InputFromFile;

		[ValueArgument(typeof(string), 'u', "url", AllowMultiple = false, Description = "Aax(c) input from http(s) url")]
		public string InputFromUrl;

		[ValueArgument(typeof(string), "user_agent", AllowMultiple = false, Description = "Http(s) user agent", FullDescription = "Default is \"Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0\"", DefaultValue = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0")]
		public string UrlUserAgent;

		[ValueArgument(typeof(Cookie), "cookie", AllowMultiple = true, Description = "Http(s) cookie", Example ="name=value")]
		public List<Cookie> Cookie;

		[ValueArgument(typeof(ActivationByteString), "activation_bytes", AllowMultiple = false, Description = "Aax file activation bytes")]
		public ActivationByteString AudibleActivationBytes;

		[ValueArgument(typeof(AaxcKeyByteString), "audible_key", AllowMultiple = false, Description = "Aaxc file key")]
		public AaxcKeyByteString AudibleKey;

		[ValueArgument(typeof(AaxcIVByteString), "audible_iv", AllowMultiple = false, Description = "Aaxc file iv")]
		public AaxcIVByteString AudibleIV;

		[ValueArgument(typeof(string), 'o', "outfile", AllowMultiple = false, Description = "Uutput file to write the decrypted m4b", Optional = false)]
		public string OutputToFile;

		public AaxFile GetInputFile()
		{
			var aaxFile = InputFromFile is not null ? new AaxFile(InputFromFile.FullName) : GetAaxFromUrl();

			if (AudibleActivationBytes is not null)
				aaxFile.SetDecryptionKey(AudibleActivationBytes.Bytes);
			else
				aaxFile.SetDecryptionKey(AudibleKey.Bytes, AudibleIV.Bytes);

			return aaxFile;
		}

		public Stream GetOutputStream()
		{
			return File.Open(OutputToFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		}

		private AaxFile GetAaxFromUrl()
		{
			var uri = new Uri(InputFromUrl);

			var httpRequest = WebRequest.CreateHttp(uri);
			httpRequest.Headers = new WebHeaderCollection
			{
				{ "User-Agent", UrlUserAgent }
			};

			httpRequest.CookieContainer = new CookieContainer();

			foreach (var c in Cookie)
			{
				httpRequest.CookieContainer.Add(uri, new System.Net.Cookie(c.Name, c.Value));
			}
			var response = httpRequest.GetResponse() as HttpWebResponse;

			var stream = response.GetResponseStream();

			return new AaxFile(stream, response.ContentLength);
		}
	}
}
