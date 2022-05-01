using CommandLineParser.Exceptions;
using System;

namespace aaxclean_cli
{
	public class Chapter
	{
		public string Title { get; private init; }
		public TimeSpan Start { get; private init; }
		public TimeSpan Duration { get; private init; }
		public static Chapter Parse(string argStr)
		{
			var split = argStr.Split('|', StringSplitOptions.RemoveEmptyEntries);

			if (split.Length!=3)
					throw new CommandLineArgumentException("Chapter format is \"Title|start_secs|duration_secs\"", "chapter");
			
			if (!double.TryParse(split[1], out double startSecs) || startSecs < 0)
				throw new CommandLineArgumentException("start_secs must be number of decimal seconds", "chapter");

			if (!double.TryParse(split[2], out double durationSecs) || durationSecs < 0)
				throw new CommandLineArgumentException("duration_secs must be number of decimal seconds", "chapter");

			return new Chapter { Title = split[0], Start = TimeSpan.FromSeconds(startSecs), Duration = TimeSpan.FromSeconds(durationSecs) };
		}
	}
}
