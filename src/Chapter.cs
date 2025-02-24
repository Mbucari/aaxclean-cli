using CommandLineParser.Exceptions;
using System;
using System.Text.Json.Serialization;

namespace aaxclean_cli
{
	public class Chapter
	{
		[JsonPropertyName("length_ms")]
		public long LengthMs { get; set; }

		[JsonPropertyName("start_offset_ms")]
		public long StartOffsetMs { get; set; }

		[JsonPropertyName("start_offset_sec")]
		public long StartOffsetSec { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		public static Chapter Parse(string argStr)
		{
			var split = argStr.Split('|', StringSplitOptions.RemoveEmptyEntries);

			if (split.Length!=3)
					throw new CommandLineArgumentException("Chapter format is \"Title|start_ms|duration_ms\"", "chapter");
			
			if (!long.TryParse(split[1], out long startMs) || startMs < 0)
				throw new CommandLineArgumentException("start_ms must be number of decimal seconds", "chapter");

			if (!long.TryParse(split[2], out long durationMs) || durationMs < 0)
				throw new CommandLineArgumentException("duration_ms must be number of decimal seconds", "chapter");

			return new Chapter { Title = split[0], StartOffsetMs = startMs, StartOffsetSec = startMs / 1000, LengthMs = durationMs };
		}
	}
}
