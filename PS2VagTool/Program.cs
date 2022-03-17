using System;

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
                if (args[0].ToLower().Contains("help") || args[0].Contains("?"))
                {
                    Console.WriteLine("PlayStation 2 Vag Tool. By jmarti856");
                    Console.WriteLine("------------------------------------For Encoding------------------------------------");
                    Console.WriteLine("Usage: <InputFile> <OutputFile>");
                    Console.WriteLine("Optioms:");
                    Console.WriteLine("-1 : force non-looping");
                    Console.WriteLine("-L : force looping");
                    Console.WriteLine("");
                    Console.WriteLine("------------------------------------For Decoding------------------------------------");
                    Console.WriteLine("Usage: <InputFile> <OutputFile>");
                }
                else
                {
                    //Get Parameters
                    bool forceLooping = false;
                    bool forceNoLooping = false;
                    if (args.Length > 1)
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
                                    string outputFile = args[2];
                                    if (ProgramFunctions.CheckDirectoryExists(outputFile))
                                    {
                                        ProgramFunctions.ExecuteDecoder(inputFile, outputFile);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Execute encoder
                            if (ProgramFunctions.CheckFileExists(inputFile))
                            {
                                string outputFile = args[1];
                                if (ProgramFunctions.CheckDirectoryExists(outputFile))
                                {
                                    //Check if we have some options setted
                                    if (args.Length > 2)
                                    {
                                        string options = args[2];
                                        if (Convert.ToChar(options.TrimStart('-')) == '1')
                                        {
                                            forceNoLooping = true;
                                        }
                                        else if (Convert.ToChar(options.TrimStart('-')) == 'L')
                                        {
                                            forceLooping = true;
                                        }
                                    }
                                    ProgramFunctions.ExecuteEncoder(inputFile, outputFile, forceNoLooping, forceLooping);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
