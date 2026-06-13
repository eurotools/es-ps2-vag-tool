using System;
using System.IO;

namespace PS2VagTool
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    class Program
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        private static void Main(string[] args)
        {
            //Ensure that we have arguments
            if (args.Length > 0)
            {
                //Show help if required
                if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("?"))
                {
                    Console.WriteLine("PlayStation 2 Vag Tool. By jmarti856");
                    Console.WriteLine("Info: Supports WAV and AIFF files, also reads the loop points inside the file format.");
                    Console.WriteLine("");
                    Console.WriteLine("------------------------------------For Encoding------------------------------------");
                    Console.WriteLine("Usage: <InputFile>");
                    Console.WriteLine("Options:");
                    Console.WriteLine("-1 : force non-looping");
                    Console.WriteLine("-L : force looping");
                    Console.WriteLine("-o <OutputFile> : set output file");
                    Console.WriteLine("--verbose : print detected format and loop info");
                    Console.WriteLine("");
                    Console.WriteLine("------------------------------------For Decoding------------------------------------");
                    Console.WriteLine("Usage: Decode <InputFile> <OutputFile>");

                }
                else
                {
                    string inputFile = args[0];
                    //Check if we have to execute the decoder
                    if (inputFile.Equals("Decode", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length > 2)
                        {
                            inputFile = args[1];
                            if (ProgramFunctions.CheckFileExists(inputFile))
                            {
                                ProgramFunctions.ExecuteDecoder(inputFile, args[2].Trim());
                            }
                        }
                    }
                    else
                    {
                        //Execute encoder
                        if (ProgramFunctions.CheckFileExists(inputFile))
                        {
                            //Get Parameters
                            bool forceLooping = false;
                            bool forceNoLooping = false;
                            bool verbose = false;
                            string outputFile = Path.ChangeExtension(inputFile, ".vag");

                            //Check if we have some options setted
                            if (args.Length > 1)
                            {
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
                                    else if ((option.Equals("-o", StringComparison.OrdinalIgnoreCase) || option.Equals("--output", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
                                    {
                                        outputFile = args[++i].Trim();
                                    }
                                    else
                                    {
                                        Console.WriteLine("WARNING: unknown option ignored: " + option);
                                    }
                                }
                            }
                            if (forceNoLooping && forceLooping)
                            {
                                Console.WriteLine("ERROR: -1 and -L cannot be used together.");
                            }
                            else
                            {
                                ProgramFunctions.ExecuteEncoder(inputFile, outputFile, forceNoLooping, forceLooping, verbose);
                            }
                        }
                    }

                }
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
