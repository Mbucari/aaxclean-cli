using System;

namespace aaxclean_cli;

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
		int index = cookie.IndexOf('|');
		if (index < 1)
			throw new Exception("Cookie format is \"name|value\"");

		var name = cookie.Substring(0, index);
		var value = cookie.Substring(index + 1);

		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
			throw new Exception("Cookie format is \"name|value\"");

		Name = name;
		Value = value;
	}
}
