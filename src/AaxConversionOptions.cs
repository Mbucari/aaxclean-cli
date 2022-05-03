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
	[DistinctGroupsCertification("audible_key,audible_iv", "activation_bytes")]
	[ArgumentGroupCertification("audible_key,audible_iv", EArgumentGroupCondition.AllOrNoneUsed)]
	[DistinctGroupsCertification("chapter,outfile", "stdout")]
	public class AaxConversionOptions
	{
		[FileArgument('f', "file", AllowMultiple = false, Description = "Aax(c) input from file", FileMustExist = true)]
		public FileInfo InputFromFile;

		[ValueArgument(typeof(string), 'u', "url", AllowMultiple = false, Description = "Aax(c) input from http(s) url")]
		public string InputFromUrl;

		[ValueArgument(typeof(string), "user_agent", AllowMultiple = false, Description = "Http(s) user agent", FullDescription = "Default is \"Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0\"", DefaultValue = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0")]
		public string UrlUserAgent;

		[ValueArgument(typeof(Cookie), "cookie", AllowMultiple = true, Description = "Http(s) cookie", Example = "--cookie \"name1|value1\" --cookie \"name2|value2\"")]
		public List<Cookie> Cookies;

		[ValueArgument(typeof(ActivationByteString), "activation_bytes", AllowMultiple = false, Description = "Aax file activation bytes", Example = "a0b1c2d3")]
		public ActivationByteString AudibleActivationBytes;

		[ValueArgument(typeof(AaxcKeyByteString), "audible_key", AllowMultiple = false, Description = "Aaxc file key", Example = "a0b1c2d3e4f5a0b1c2d3e4f5a0b1c2d3")]
		public AaxcKeyByteString AudibleKey;

		[ValueArgument(typeof(AaxcIVByteString), "audible_iv", AllowMultiple = false, Description = "Aaxc file iv", Example = "c2d3e4f5a0b1c2d3a0b1c2d3e4f5a0b1")]
		public AaxcIVByteString AudibleIV;

		[ValueArgument(typeof(Chapter), "chapter", AllowMultiple = true, Description = "user-defined chapter marker: Title|start_secs|duration_secs", Example = "--chapter \"Chapter 1|0|1345.245\" --chapter \"Chapter 2|1345.245|2411.579\"")]
		public List<Chapter> Chapters;

		[FileArgument('o', "outfile", AllowMultiple = false, Description = "Output file to write the decrypted m4b", Optional = true, FileMustExist = false)]
		public FileInfo OutputToFile;

		[SwitchArgument("stdout", false, Description = "Write output to stdout. No file fixups or custom chapters allowed with this option.", Optional = true)]
		public bool StandardOut;

		[SwitchArgument("list-chapters", false, Description = "List the chapters from metadata", Optional = true)]
		public bool ListChapters;

		public AaxFile GetInputFile()
		{
			var aaxFile = InputFromFile is not null ? GetAaxFromFile() : GetAaxFromUrl();

			if (AudibleActivationBytes is not null)
				aaxFile.SetDecryptionKey(AudibleActivationBytes.Bytes);
			else if (AudibleKey is not null && AudibleIV is not null)
				aaxFile.SetDecryptionKey(AudibleKey.Bytes, AudibleIV.Bytes);

			return aaxFile;
		}

		public ChapterInfo GetUserChapters()
		{
			if (Chapters.Count == 0) return null;

			Chapters.Sort((c1, c2) => c1.Start.CompareTo(c2.Start));

			var chInfo = new ChapterInfo();

			foreach (var c in Chapters)
				chInfo.AddChapter(c.Title, c.Duration);

			return chInfo;
		}

		public Stream GetOutputStream()
		{
			if (StandardOut)
				return Console.OpenStandardOutput(4 * 1024 * 1024);

			if (!OutputToFile.Directory.Exists)
				OutputToFile.Directory.Create();
			return OutputToFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		}
		private AaxFile GetAaxFromFile()
		{
			var inFile = InputFromFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

			return new AaxFile(inFile, inFile.Length, !StandardOut);
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

			foreach (var c in Cookies)
			{
				httpRequest.CookieContainer.Add(uri, new System.Net.Cookie(c.Name, c.Value));
			}
			var response = httpRequest.GetResponse() as HttpWebResponse;

			var stream = response.GetResponseStream();

			return new AaxFile(stream, response.ContentLength, !StandardOut);
		}
	}
}
