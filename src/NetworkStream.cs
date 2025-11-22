using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace aaxclean_cli;

internal class NetworkStream : Stream
{
	HttpClient Client { get; }
	public Uri Location { get; }
	private HttpResponseMessage Response { get; set; }
	private long length = -1;
	private long position = 0;
	public NetworkStream(string url, IEnumerable<string> userAgent, IEnumerable<Cookie> cookies)
	{
		Location = new Uri(url);

		var cookieContainer = new CookieContainer();
		Client = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer });
		foreach (var c in cookies)
		{
			cookieContainer.Add(Location, new System.Net.Cookie(c.Name, c.Value));
		}
		Client.DefaultRequestHeaders.Add("User-Agent", userAgent);
		Client.DefaultRequestHeaders.Add("Range", "bytes=0-");
		var request = new HttpRequestMessage(HttpMethod.Get, Location);
		Response = Client.Send(request, HttpCompletionOption.ResponseHeadersRead);
		length = Response.Content.Headers.ContentLength ?? throw new Exception("Could not determine content length from response");
		currentStream = Response.Content.ReadAsStream();
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;
	public override long Length => length;

	public override long Position { get => position; set => throw new NotImplementedException(); }

	private Stream currentStream = null;
	public override int Read(byte[] buffer, int offset, int count)
	{
		while (Position < Length)
		{
			try
			{
				int read = currentStream.Read(buffer, offset, count);
				position += read;
				return read;
			}
			catch (HttpIOException e) when (e.HttpRequestError is HttpRequestError.ResponseEnded && Position < Length)
			{
				Client.DefaultRequestHeaders.Remove("Range");
				Client.DefaultRequestHeaders.Add("Range", $"bytes={position}-");
				currentStream?.Dispose();
				Response?.Dispose();
				var request = new HttpRequestMessage(HttpMethod.Get, Location);
				Response = Client.Send(request, HttpCompletionOption.ResponseHeadersRead);
				currentStream = Response.Content.ReadAsStream();
			}
		}
		throw new EndOfStreamException();
	}
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		Response?.Dispose();
		Client?.Dispose();
	}
	public override void Flush()
	{
		throw new NotImplementedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
