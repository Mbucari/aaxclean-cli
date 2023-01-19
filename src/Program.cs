﻿using AAXClean;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace aaxclean_cli
{
	class Program
	{
		private static readonly TextWriter ConsoleText = Console.Error;
		public static async Task<int> Main(string[] args)
		{
			CommandLineParser.CommandLineParser parser = new();

			AaxConversionOptions aaxConversionOptions = new();

			parser.ExtractArgumentAttributes(aaxConversionOptions);

			if (args is null || args.Length == 0)
			{
				parser.PrintUsage(ConsoleText);
				return await Task.FromResult(-1);
			}
			try
			{
				parser.ParseCommandLine(args);
			}
			catch (CommandLineParser.Exceptions.CommandLineException ex)
			{
				WriteColoredLine((ex.Message, ConsoleColor.Red));

				return await Task.FromResult(-1);
			}
			catch (Exception ex)
			{
				WriteColoredLine(
					   ("Error", ConsoleColor.Red),
					   (": ", ConsoleColor.White),
					   (ex.Message, ConsoleColor.DarkRed));

				return await Task.FromResult(-2);
			}

			try
			{
				var chapters = aaxConversionOptions.GetUserChapters();
				if (chapters is not null)
					ListChapters("JSON FILE CHAPTERS", chapters);

				ReWriteColored(($"Opening Aax file...", ConsoleColor.White));
				var aaxFile = aaxConversionOptions.GetInputFile();
				ReWriteColored(($"Aax file opened successfully\r\n", ConsoleColor.Green));

				if (aaxConversionOptions.ListChapters)
				{
					ListChapters("CHAPTER LIST", aaxFile.GetChaptersFromMetadata());
				}

				if (aaxConversionOptions.OutputToFile is null) return 0;

				if (aaxFile.Key is null || aaxFile.IV is null)
				{
					WriteColoredLine(("ERROR: Aax key file not found", ConsoleColor.Red));
					return -3;
				}


				DateTime startTime = DateTime.Now;
				int chNum = 1;
				var operation
					= aaxConversionOptions.SplitFileByChapters
					? aaxFile.ConvertToMultiMp4aAsync(chapters ?? aaxFile.GetChaptersFromMetadata(), cb => cb.OutputFile = aaxConversionOptions.GetOutputStream(chNum++))
					: aaxFile.ConvertToMp4aAsync(aaxConversionOptions.GetOutputStream(), chapters);

				operation.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;

				await operation;

				var duration = DateTime.Now - startTime;


				ConsoleText.WriteLine();
				WriteColoredLine(
					("\r\nConversion succeeded!", ConsoleColor.Green),
					($"  Total time: {duration:mm\\:ss\\.ff}", ConsoleColor.White));

				return 0;

			}
			catch (Exception ex)
			{
				ConsoleText.WriteLine();
				WriteColoredLine(
					("Error Converting Book", ConsoleColor.Red),
					(": ", ConsoleColor.White),
					(ex.Message, ConsoleColor.DarkRed));

				return await Task.FromResult(-2); ;
			}
		}
		
		private static void ListChapters(string prefix, AAXClean.ChapterInfo chInfo)
		{
			if (chInfo is null)
			{
				WriteColoredLine(("Error reading chapters from metadata", ConsoleColor.Red));
			}

			int maxLen = chInfo.Max(c => c.Title.Length) + 3;

			WriteColoredLine(($"\r\n{prefix}", ConsoleColor.Magenta));

			WritePad('-', maxLen + 67);
			ConsoleText.WriteLine();

			foreach (var ch in chInfo)
			{
				WriteColored(($"\"{ch.Title}\"", ConsoleColor.Yellow));
				WritePad(' ', maxLen - ch.Title.Length);
				WriteColoredLine(
					($"Start", ConsoleColor.Green),
					($" = {(int)ch.StartOffset.TotalHours:D2}:{ch.StartOffset:mm\\:ss\\.fff}, ", ConsoleColor.White),
					($"End", ConsoleColor.Green),
					($" = {(int)ch.EndOffset.TotalHours:D2}:{ch.EndOffset:mm\\:ss\\.fff}, ", ConsoleColor.White),
					($"Duration", ConsoleColor.Green),
					($" = {ch.Duration:hh\\:mm\\:ss\\.fff}", ConsoleColor.White));
			}

			WritePad('-', maxLen + 67);
			ConsoleText.WriteLine();
		}

		private static void AaxFile_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
		{
			ReWriteColored
				(
				("Conversion progress", ConsoleColor.Green),
				($": {e.ProcessPosition / e.TotalDuration * 100:F2}%    ", ConsoleColor.White),
				("average speed", ConsoleColor.Green),
				($" = {(int)e.ProcessSpeed}x", ConsoleColor.White)
				);
		}

		static int lastUpdateLength = 0;

		private static void ReWriteColored(params (string str, ConsoleColor color)[] coloredText)
		{
			int textLen = 0;
			ConsoleText.Write('\r');
			foreach (var (str, color) in coloredText)
			{
				textLen += str.Length;
				Console.ForegroundColor = color;
				ConsoleText.Write(str);
			}
			Console.ResetColor();

			int endPad = Math.Max(0, lastUpdateLength - textLen);
			WritePad(' ', endPad);

			lastUpdateLength = textLen - endPad;
		}
		private static void WriteColoredLine(params (string str, ConsoleColor color)[] coloredText)
		{
			WriteColored(coloredText);
			ConsoleText.WriteLine();
		}

		private static void WriteColored(params (string str, ConsoleColor color)[] coloredText)
		{
			foreach (var ct in coloredText)
			{
				Console.ForegroundColor = ct.color;
				ConsoleText.Write(ct.str);
			}
			Console.ResetColor();
		}

		static void WritePad(char c, int padLen)
		{
			for (int i = 0; i < padLen; i++)
				ConsoleText.Write(c);
		}
	}
}
