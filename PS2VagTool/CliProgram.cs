using System;
using System.Globalization;
using System.IO;

namespace PS2VagTool
{
    internal static class CliProgram
    {
        private const int DefaultInterleaveSize = 16;
        private const int MaximumInterleaveSize = 0x100000;

        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 2;
            }

            if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("?"))
            {
                PrintHelp();
                return 0;
            }

            return args[0].Equals("Decode", StringComparison.OrdinalIgnoreCase) ? RunDecoder(args) : RunEncoder(args);
        }

        private static int RunDecoder(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("ERROR: Decode requires an input VAG and an output WAV path.");
                return 2;
            }

            if (!ProgramFunctions.CheckFileExists(args[1]))
            {
                return 1;
            }

            int interleaveSize = DefaultInterleaveSize;
            int? sampleFrames = null;
            for (int i = 3; i < args.Length; i++)
            {
                string option = args[i];
                if (option.Equals("-i", StringComparison.OrdinalIgnoreCase) || option.Equals("--interleave", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length || !TryParseInterleaveSize(args[++i], out interleaveSize))
                    {
                        PrintInterleaveError();
                        return 2;
                    }
                }
                else if (option.Equals("--samples", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length || !Int32.TryParse(args[++i], NumberStyles.None, CultureInfo.InvariantCulture, out int frames) || frames < 0)
                    {
                        Console.WriteLine("ERROR: --samples must be a non-negative number of PCM frames.");
                        return 2;
                    }

                    sampleFrames = frames;
                }
                else
                {
                    Console.WriteLine("ERROR: unknown option: " + option);
                    return 2;
                }
            }

            return ProgramFunctions.ExecuteDecoder(args[1], args[2].Trim(), interleaveSize, sampleFrames) ? 0 : 1;
        }

        private static int RunEncoder(string[] args)
        {
            if (!ProgramFunctions.CheckFileExists(args[0]))
            {
                return 1;
            }

            bool forceLooping = false;
            bool forceNoLooping = false;
            bool verbose = false;
            int interleaveSize = DefaultInterleaveSize;
            string outputFile = Path.ChangeExtension(args[0], ".vag");

            for (int i = 1; i < args.Length; i++)
            {
                string option = args[i];
                if (option.Equals("-1", StringComparison.OrdinalIgnoreCase))
                {
                    forceNoLooping = true;
                }
                else if (option.Equals("-L", StringComparison.OrdinalIgnoreCase))
                {
                    forceLooping = true;
                }
                else if (option.Equals("--verbose", StringComparison.OrdinalIgnoreCase) || option.Equals("-v", StringComparison.OrdinalIgnoreCase))
                {
                    verbose = true;
                }
                else if (option.Equals("-o", StringComparison.OrdinalIgnoreCase) || option.Equals("--output", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length || String.IsNullOrWhiteSpace(args[i + 1]))
                    {
                        Console.WriteLine("ERROR: " + option + " requires an output file path.");
                        return 2;
                    }
                    outputFile = args[++i].Trim();
                }
                else if (option.Equals("-i", StringComparison.OrdinalIgnoreCase) || option.Equals("--interleave", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length || !TryParseInterleaveSize(args[++i], out interleaveSize))
                    {
                        PrintInterleaveError();
                        return 2;
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: unknown option: " + option);
                    return 2;
                }
            }

            if (forceNoLooping && forceLooping)
            {
                Console.WriteLine("ERROR: -1 and -L cannot be used together.");
                return 2;
            }

            return ProgramFunctions.ExecuteEncoder(args[0], outputFile, forceNoLooping, forceLooping, verbose, interleaveSize) ? 0 : 1;
        }

        private static bool TryParseInterleaveSize(string value, out int interleaveSize)
        {
            interleaveSize = 0;
            if (String.IsNullOrWhiteSpace(value)) return false;

            bool parsed = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? Int32.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out interleaveSize)
                : Int32.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out interleaveSize);

            return parsed && interleaveSize > 0 && interleaveSize <= MaximumInterleaveSize && interleaveSize % 16 == 0;
        }

        private static void PrintInterleaveError()
        {
            Console.WriteLine("ERROR: interleave must be a positive multiple of 16 no greater than 0x100000 (for example 16, 128 or 0x80).");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("PlayStation 2 Vag Tool. By jmarti856");
            Console.WriteLine("Encoding: AIFF2VAG.exe <InputFile> [options]");
            Console.WriteLine("  -1                         Force non-looping");
            Console.WriteLine("  -L                         Force looping");
            Console.WriteLine("  -o, --output <file>        Set output file");
            Console.WriteLine("  -i, --interleave <bytes>   Stereo interleave (default 16; accepts 128 or 0x80)");
            Console.WriteLine("  -v, --verbose              Print format and loop information");
            Console.WriteLine();
            Console.WriteLine("Decoding: AIFF2VAG.exe Decode <InputFile> <OutputFile> [options]");
            Console.WriteLine("  -i, --interleave <bytes>   Stereo interleave used by the input VAG");
            Console.WriteLine("  --samples <frames>         Trim block padding to an exact PCM frame count");
        }
    }
}
