using AAXClean;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace aaxclean_cli;

public partial class AaxConversionOptions
{
	public string InputFromFile { get; private set; }
	public string InputFromUrl { get; private set; }
	public List<string> UrlUserAgent { get; private set; } = new();
	public List<Cookie> Cookies { get; private set; } = new();
	public FixedLengthByteString AudibleActivationBytes { get; private set; }
	public FixedLengthByteString AudibleKey { get; private set; }
	public FixedLengthByteString AudibleIV { get; private set; }
	public List<Chapter> Chapters { get; private set; } = new();
	public string ChapterInfoFile { get; private set; }
	public string OutputToFile { get; private set; }
	public bool SplitFileByChapters { get; private set; }
	public bool ListChapters { get; private set; }
	public int ReturnCode { get; private set; }

	public AaxFile GetInputFile()
	{
		var aaxFile = File.Exists(InputFromFile) ? GetAaxFromFile() : GetAaxFromUrl();

		if (AudibleActivationBytes is not null)
			aaxFile.SetDecryptionKey(AudibleActivationBytes.Bytes);
		else if (AudibleKey is not null && AudibleIV is not null)
			aaxFile.SetDecryptionKey(AudibleKey.Bytes, AudibleIV.Bytes);

		return aaxFile;
	}

	public AAXClean.ChapterInfo GetUserChapters()
	{
		if (Chapters.Count != 0) return GetIndividualChapters();
		else if (File.Exists(ChapterInfoFile)) return GetJsonChapters();
		return null;
	}

	private AAXClean.ChapterInfo GetJsonChapters()
	{
		try
		{
			string json = File.ReadAllText(ChapterInfoFile);

			var audible_chInfo = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.ChapterInfo);

			Array.Sort(audible_chInfo.Chapters, (c1, c2) => c1.StartOffsetMs.CompareTo(c2.StartOffsetMs));

			var chInfo = new AAXClean.ChapterInfo();

			foreach (var c in audible_chInfo.Chapters)
				chInfo.AddChapter(c.Title, TimeSpan.FromMilliseconds(c.LengthMs));

			return chInfo;
		}
		catch (Exception ex) { throw new ArgumentException("Failed to parse chapter_info json file", ex); }
	}

	private AAXClean.ChapterInfo GetIndividualChapters()
	{
		var chInfo = new AAXClean.ChapterInfo();

		foreach (var c in Chapters.OrderBy(c => c.StartOffsetMs))
			chInfo.AddChapter(c.Title, TimeSpan.FromMilliseconds(c.LengthMs));

		return chInfo;
	}

	public Stream GetOutputStream()
	{
		var outDir = Path.GetDirectoryName(OutputToFile);
		if (!Directory.Exists(outDir))
			Directory.CreateDirectory(outDir);
		return File.Open(OutputToFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
	}

	public Stream GetOutputStream(int chapter)
	{
		var outDir = Path.GetDirectoryName(OutputToFile);
		var fileName = Path.GetFileName(OutputToFile);
		if (!Directory.Exists(outDir))
			Directory.CreateDirectory(outDir);
		return File.Open(Path.Combine(outDir, $"{chapter:d2} - {fileName}"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
	}

	private AaxFile GetAaxFromFile()
	{
		var inFile = File.Open(InputFromFile, FileMode.Open, FileAccess.Read, FileShare.Read);

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

		return new AaxFile(stream, response.Content.Headers.ContentLength.Value);
	}
}
