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

            AaxConversionOptions p = new();            

            parser.ExtractArgumentAttributes(p);

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
                Console.Error.WriteLine(ex.Message);
                return -1;
            }
			catch(Exception ex)
			{
                Console.Error.WriteLine($"Error: {ex.Message}");
                return -2;
			}

			try
            {
                var chapters = p.GetUserChapters();

                WriteProgressUpdate($"Opening aax file...");

                var aaxFile = p.GetInputFile();
				aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;

                DateTime startTime = DateTime.Now;
                var result = aaxFile.ConvertToMp4a(p.GetOutputStream(), chapters);
                var duration = DateTime.Now - startTime;

                if (result == ConversionResult.Failed)
				{
                    Console.Error.WriteLine($"\r\nConversion Failed!");
                    return -3;
                }
				else
                {
                    Console.Error.WriteLine($"\r\nConversion succeeded! Total time: {duration:mm\\:ss\\.ff}");
                    return 0;
                }
            }
            catch(Exception ex)
			{
                Console.Error.WriteLine($"Error Converting Book: {ex.Message}");
                return -3;
			}
        }

		private static void AaxFile_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
        {
            WriteProgressUpdate($"Conversion progress: {e.ProcessPosition / e.TotalDuration * 100:F2}%    average speed = {(int)e.ProcessSpeed}x");
        }

        static int lastUpdateLength = 0;
        private static void WriteProgressUpdate(string progressString)
		{
            StringBuilder sb = new();

            sb.Append('\r');
            sb.Append(progressString);

            int endPad = Math.Max(0, lastUpdateLength - sb.Length);

            sb.Append(' ', endPad);

            lastUpdateLength = sb.Length - endPad;
            Console.Error.Write(sb.ToString());
        }
    }
}
