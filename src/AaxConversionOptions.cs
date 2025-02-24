using AAXClean;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace aaxclean_cli
{

	[DistinctGroupsCertification("url,user_agent,cookie", "file")]
	[ArgumentGroupCertification("file,url", EArgumentGroupCondition.ExactlyOneUsed)]
	[DistinctGroupsCertification("audible_key,audible_iv", "activation_bytes")]
	[ArgumentGroupCertification("audible_key,audible_iv", EArgumentGroupCondition.AllOrNoneUsed)]
	[ArgumentGroupCertification("chapter,chapter_info,list-chapters", EArgumentGroupCondition.OneOreNoneUsed)]
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

		[ValueArgument(typeof(Chapter), "chapter", AllowMultiple = true, Description = "user-defined chapter marker: Title|start_ms|duration_ms", Example = "--chapter \"Chapter 1|0|1345245\" --chapter \"Chapter 2|1345245|2411579\"")]
		public List<Chapter> Chapters;

		[FileArgument("chapter_info", AllowMultiple = false, Description = "file path to an Audible Api chapter_info json structure", Optional = true, FileMustExist = true)]
		public FileInfo ChapterInfoFile;

		[FileArgument('o', "outfile", AllowMultiple = false, Description = "Output file to write the decrypted m4b", Optional = true, FileMustExist = false)]
		public FileInfo OutputToFile;

		[SwitchArgument('s', false, Description = "Split file int myltiple files by chapters\r\nIf this option is specified, output file names are prepended with the chapter number.", Optional = true)]
		public bool SplitFileByChapters;

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

		public AAXClean.ChapterInfo GetUserChapters()
		{
			if (Chapters.Count != 0) return GetIndividualChapters();
			else if (ChapterInfoFile is not null) return GetJsonChapters();
			return null;
		}

		private AAXClean.ChapterInfo GetJsonChapters()
		{
			try
			{
				string json = File.ReadAllText(ChapterInfoFile.FullName);
				var audible_chInfo = JsonConvert.DeserializeObject<ChapterInfo>(json);

				Array.Sort(audible_chInfo.Chapters, (c1, c2) => c1.StartOffsetMs.CompareTo(c2.StartOffsetMs));

				var chInfo = new AAXClean.ChapterInfo();

				foreach (var c in audible_chInfo.Chapters)
					chInfo.AddChapter(c.Title, TimeSpan.FromMilliseconds(c.LengthMs));

				return chInfo;
			}
			catch (Exception ex) { throw new ArgumentException("Failed to parse chapterinfo json file", ex); }
		}

		private AAXClean.ChapterInfo GetIndividualChapters()
		{

			Chapters.Sort((c1, c2) => c1.StartOffsetMs.CompareTo(c2.StartOffsetMs));

			var chInfo = new AAXClean.ChapterInfo();

			foreach (var c in Chapters)
				chInfo.AddChapter(c.Title, TimeSpan.FromMilliseconds(c.LengthMs));

			return chInfo;
		}

		public Stream GetOutputStream()
		{
			if (!OutputToFile.Directory.Exists)
				OutputToFile.Directory.Create();
			return OutputToFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		}

		public Stream GetOutputStream(int chapter)
		{
			if (!OutputToFile.Directory.Exists)
				OutputToFile.Directory.Create();
			return File.Open(Path.Combine(OutputToFile.Directory.FullName,$"{chapter:d2} - {OutputToFile.Name}"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		}

		private AaxFile GetAaxFromFile()
		{
			var inFile = InputFromFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

			return new AaxFile(inFile, inFile.Length);
		}

		private AaxFile GetAaxFromUrl()
		{
			var uri = new Uri(InputFromUrl);

			var cookieContainer = new CookieContainer();
			using var handler = new HttpClientHandler { CookieContainer = cookieContainer };
			using var client = new HttpClient(handler);
			using var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Add("User-Agent", UrlUserAgent);			

			foreach (var c in Cookies)
			{
				cookieContainer.Add(uri, new System.Net.Cookie(c.Name, c.Value));
			}

			var response = client.Send(request, HttpCompletionOption.ResponseHeadersRead);
			var stream = response.Content.ReadAsStream();

			return new AaxFile(stream,  response.Content.Headers.ContentLength.Value);
		}
	}
}
