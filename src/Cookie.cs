﻿using CommandLineParser.Exceptions;

namespace aaxclean_cli
{
	public class Cookie
	{
		public string Name { get; private init; }
		public string Value { get; private init; }
		public override string ToString()
		{
			return $"{Name}={Value}";
		}

		public static Cookie Parse(string stringValue)
		{
			var split = stringValue.Split('|', System.StringSplitOptions.RemoveEmptyEntries);

			if (split.Length != 2)
				throw new CommandLineArgumentException("Cookie format is \"name|value\"", "cookie");

			split[0] = split[0].Trim();
			split[1] = split[1].Trim();

			if (string.IsNullOrEmpty(split[0]) || string.IsNullOrEmpty(split[1]))
				throw new CommandLineArgumentException("Cookie format is \"name|value\"", "cookie");
			return new Cookie { Name = split[0], Value = split[1] };
		}
	}
}
