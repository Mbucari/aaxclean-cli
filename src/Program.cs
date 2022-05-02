using AAXClean;
using System;
using System.Linq;
using System.Text;

namespace aaxclean_cli
{
    class Program
    {
        public static int Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new();

            AaxConversionOptions aaxConversionOptions = new();            

            parser.ExtractArgumentAttributes(aaxConversionOptions);

            if (args is null || args.Length == 0 )
			{
                parser.PrintUsage(Console.Error);
                return -1;
            }
            try
            {
                parser.ParseCommandLine(args);
            }
            catch (CommandLineParser.Exceptions.CommandLineException ex)
            {
                WriteColoredLine(
                       (ex.Message, ConsoleColor.Red)
                       );

                return -1;
            }
			catch(Exception ex)
			{
                WriteColoredLine(
                       ("Error", ConsoleColor.Red),
                       (": ", ConsoleColor.White),
                       (ex.Message, ConsoleColor.DarkRed)
                       );

                return -2;
			}

			try
            {
                var chapters = aaxConversionOptions.GetUserChapters();

                ReWriteColored(($"Opening Aax file...", ConsoleColor.White));
                var aaxFile = aaxConversionOptions.GetInputFile();
                ReWriteColored(($"Aax file opened successfully\r\n", ConsoleColor.Green));

                if (aaxConversionOptions.ListChapters)
                {
                    ListChapters(aaxFile);
                }

                if (aaxConversionOptions.OutputToFile is null) return 0;

                if (aaxFile.Key is null || aaxFile.IV is null)
                {
                    WriteColoredLine(("ERROR: Aax key file not found", ConsoleColor.Red));
                    return -3;
                }


                aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;

                DateTime startTime = DateTime.Now;
                var result = aaxFile.ConvertToMp4a(aaxConversionOptions.GetOutputStream(), chapters);
                var duration = DateTime.Now - startTime;

                Console.Error.WriteLine();

                if (result == ConversionResult.Failed)
                {
                    WriteColoredLine(("Conversion Failed!", ConsoleColor.Red));
                    return -3;
                }
				else
                {
                    WriteColoredLine(
                        ("\r\nConversion succeeded!", ConsoleColor.Green),
                        ($"  Total time: {duration:mm\\:ss\\.ff}", ConsoleColor.White)
                        );

                    Console.Error.WriteLine();

                    return 0;
                }
            }
            catch(Exception ex)
            {
                WriteColoredLine(
                    ("Error Converting Book", ConsoleColor.Red),
                    (": ", ConsoleColor.White),
                    (ex.Message, ConsoleColor.DarkRed)
                    );

                return -3;
			}
        }

        private static void ListChapters(AaxFile aaxFile)
		{
            var chInfo = aaxFile.GetChaptersFromMetadata();
            if (chInfo is null)
            {
                WriteColoredLine(("Error reading chapters from metadata", ConsoleColor.Red));
			}

            int maxLen = chInfo.Max(c => c.Title.Length) + 3;

            WriteColoredLine(("\r\nCHAPTER LIST", ConsoleColor.Magenta));

            WritePad('-', maxLen + 67);
            Console.Error.WriteLine();

            foreach (var ch in chInfo)
            {
                WriteColored(($"\"{ch.Title}\"", ConsoleColor.Yellow));
                WritePad(' ', maxLen - ch.Title.Length);
                WriteColored(
                    ($"Start", ConsoleColor.Green),
                    ($" = {(int)ch.StartOffset.TotalHours:D2}:{ch.StartOffset:mm\\:ss\\.fff}, ", ConsoleColor.White),
                    ($"End", ConsoleColor.Green),
                    ($" = {(int)ch.EndOffset.TotalHours:D2}:{ch.EndOffset:mm\\:ss\\.fff}, ", ConsoleColor.White),
                    ($"Duration", ConsoleColor.Green),
                    ($" = {ch.Duration:hh\\:mm\\:ss\\.fff}", ConsoleColor.White));

                Console.Error.WriteLine();
            }

            WritePad('-', maxLen + 67);
            Console.Error.WriteLine();
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
            Console.Error.Write('\r');
            foreach (var ct in coloredText)
            {
                textLen += ct.str.Length;
                Console.ForegroundColor = ct.color;
                Console.Error.Write(ct.str);
            }
            Console.ResetColor();

            int endPad = Math.Max(0, lastUpdateLength - textLen);
            WritePad(' ', endPad);

            lastUpdateLength = textLen - endPad;
        }
        private static void WriteColoredLine(params (string str, ConsoleColor color)[] coloredText)
        {
            WriteColored(coloredText);
            Console.Error.WriteLine();
        }

        private static void WriteColored(params (string str, ConsoleColor color)[] coloredText)
        {
            foreach (var ct in coloredText)
			{
                Console.ForegroundColor = ct.color;
                Console.Error.Write(ct.str);
            }
            Console.ResetColor();
        }

        static void WritePad(char c, int padLen)
		{
            for (int i = 0; i < padLen; i++)
                Console.Error.Write(c);
		}
    }
}
