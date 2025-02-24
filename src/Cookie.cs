using System;

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

		public Cookie(string cookie)
		{
			if (cookie is null)
				throw new ArgumentNullException(nameof(cookie));

			var split = cookie.Split('|', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 2)
				throw new Exception("Cookie format is \"name|value\"");

			split[0] = split[0].Trim();
			split[1] = split[1].Trim();

			if (string.IsNullOrEmpty(split[0]) || string.IsNullOrEmpty(split[1]))
				throw new Exception("Cookie format is \"name|value\"");

			Name = split[0];
			Value = split[1];
		}
	}
}
