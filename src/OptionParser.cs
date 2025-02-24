using System;
using System.IO;

namespace aaxclean_cli
{
	public partial class AaxConversionOptions
	{
		private const string DefaultUserAgent = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0";
		public static AaxConversionOptions Parse(string[] args)
		{
			bool hasOutput = false;
			bool hasInput = false;
			bool hasJsonChapters = false;

			var options = new AaxConversionOptions();

			for (int i = 0; i < args.Length; i++)
			{
				var option = args[i].Trim();

				if (option == "-s")
				{
					options.SplitFileByChapters = true;
					continue;
				}
				else if (option == "--list-chapters")
				{
					options.ListChapters = true;
					continue;
				}

				//Anything else should be an option with an argument
				switch (option.ToLowerInvariant())
				{
					case "-f":
					case "--file":
						if (hasInput)
							throw new Exception($"Only one input (either aax(c) file or url) may be specified at a time");
						options.InputFromFile = ParseAndValidateInputFile(GetNextArgument());
						hasInput = true;
						break;
					case "-u":
					case "--url":
						if (hasInput)
							throw new Exception($"Only one input (either aax(c) file or url) may be specified at a time");
						options.InputFromUrl = ParseAndValidateUrl(GetNextArgument());
						hasInput = true;
						break;
					case "--user_agent":
						options.UrlUserAgent.Add(GetNextArgument());
						break;
					case "--cookie":
						options.Cookies.Add(new Cookie(GetNextArgument()));
						break;
					case "--activation_bytes":
						var activation_bytes = GetNextArgument();
						options.AudibleActivationBytes = new FixedLengthByteString(activation_bytes, 4, "activation_bytes");
						break;
					case "--audible_key":
						var audible_key = GetNextArgument();
						options.AudibleKey = new FixedLengthByteString(audible_key, 16, "audible_key");
						break;
					case "--audible_iv":
						var audible_iv = GetNextArgument();
						options.AudibleIV = new FixedLengthByteString(audible_iv, 16, "audible_iv");
						break;
					case "--chapter":
						var chapter = GetNextArgument();
						options.Chapters.Add(new Chapter(chapter));
						break;
					case "--chapter_info":
						if (hasJsonChapters)
							throw new Exception($"Only one input (either aax(c) file or url) may be specified at a time");
						options.ChapterInfoFile = ParseAndValidateInputFile(GetNextArgument());
						hasJsonChapters = true;
						break;
					case "-o":
					case "--outfile":
						if (hasOutput)
							throw new Exception($"Only one output file may be specified");
						options.OutputToFile = ParseAndValidateOutputFile(GetNextArgument());
						hasOutput = true;
						break;
						default:
							throw new Exception($"Unknown option '{option}'");
				}

				string GetNextArgument()
				{
					if (args.Length <= ++i)
						throw new Exception($"Missing argument for {option} option");
					return args[i];
				}
			}

			if (options.AudibleActivationBytes != null && (options.AudibleKey != null || options.AudibleIV != null))
				throw new Exception("Specify either activation_bytes or audible_key and audible_iv");

			if (options.AudibleKey == null ^ options.AudibleIV == null)
				throw new Exception("audible_key and audible_iv must be specified together");

			if (options.UrlUserAgent.Count == 0)
				options.UrlUserAgent.Add(DefaultUserAgent);

			return options;
		}

		public static void PrintUsage()
		{
			string usage = $"""
								Usage:
				        -f, --file[optional]... Aax(c) input from file

				        -u, --url[optional]... Aax(c) input from http(s) url

				        --user_agent[optional]... Http(s) user agent

				Default is "{DefaultUserAgent}"

				        --cookie[optional]... Http(s) cookie
				Example: --cookie "name1|value1" --cookie "name2|value2"

				        --activation_bytes[optional]... Aax file activation bytes (8-digit hex string)
				Example: a0b1c2d3

				        --audible_key[optional]... Aaxc file key (32-digit hex string)
				Example: a0b1c2d3e4f5a0b1c2d3e4f5a0b1c2d3

				        --audible_iv[optional]... Aaxc file iv (32-digit hex string)
				Example: c2d3e4f5a0b1c2d3a0b1c2d3e4f5a0b1

				        --chapter[optional]... user-defined chapter marker: Title|start_ms|duration_ms
				Example: --chapter "Chapter 1|0|1345245" --chapter "Chapter 2|1345245|2411579"

				        --chapter_info[optional]... file path to an Audible Api chapter_info json structure

				        -o, --outfile[optional]... Output file to write the decrypted m4b

				        -s[optional]... Split file int myltiple files by chapters
				If this option is specified, output file names are prepended with the chapter number.

				        --list-chapters[optional]... List the chapters from metadata
				""";
			Console.Error.WriteLine(usage);
		}

		private static string ParseAndValidateOutputFile(string value)
		{
			var dir = Path.GetDirectoryName(value);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			return value;
		}

		private static string ParseAndValidateInputFile(string value)
			=> File.Exists(value) ? value : throw new Exception($"Input file does not exist '{value}'");
		private static string ParseAndValidateUrl(string value)
			=> Uri.TryCreate(value, default, out var uri) ? value : throw new Exception($"Invalid url '{value}'");
	}
}
