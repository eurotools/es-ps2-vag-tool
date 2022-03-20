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
                    Console.WriteLine("Optioms:");
                    Console.WriteLine("-1 : force non-looping");
                    Console.WriteLine("-L : force looping");
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

                            //Check if we have some options setted
                            if (args.Length > 1)
                            {
                                string options = args[1];
                                if (char.Parse(options.TrimStart('-')) == '1')
                                {
                                    forceNoLooping = true;
                                }
                                else if (char.Parse(options.TrimStart('-')) == 'L')
                                {
                                    forceLooping = true;
                                }
                            }
                            ProgramFunctions.ExecuteEncoder(inputFile, Path.ChangeExtension(inputFile, ".vag"), forceNoLooping, forceLooping);
                        }
                    }
                    
                }
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
