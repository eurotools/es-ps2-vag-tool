using NAudio.Wave;
using PS2VagTool.Vag_Functions;
using System;
using System.Collections.Generic;
using System.IO;

namespace PS2VagTool
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    internal static class ProgramFunctions
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        internal static void ExecuteEncoder(string inputFile, string outputFile, bool forceNoLooping, bool forceLooping)
        {
            //Variables to store audio info
            short[] pcmData = null;
            int frequency = 0, channels = 0;
            uint loopStartValue = 0, loopEndValue = 0;
            bool isLooped = false;

            //Inspect input file and get data
            string fileExtension = Path.GetExtension(inputFile);
            if (fileExtension.Equals(".aif", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals(".aiff", StringComparison.OrdinalIgnoreCase))
            {
                //Get markers
                List<MarkerChunkData> markers = new List<MarkerChunkData>();
                AiffFileChunksReader.ReadAiffHeader(File.OpenRead(inputFile), markers);
                if (markers.Count > 1)
                {
                    loopStartValue = SonyVag.GetLoopOffsetForVag(markers[0].position) - 1;
                    loopEndValue = SonyVag.GetLoopOffsetForVag(markers[1].position) - 2;
                }

                //Read file data
                using (AiffFileReader reader = new AiffFileReader(inputFile))
                {
                    //Get basic info
                    frequency = reader.WaveFormat.SampleRate;
                    channels = reader.WaveFormat.Channels;

                    //Get pcm short array
                    byte[] pcmByteData = new byte[reader.Length];
                    reader.Read(pcmByteData, 0, pcmByteData.Length);
                    pcmData = WavFunctions.ConvertByteArrayToShortArray(pcmByteData);
                }

                isLooped = markers.Count > 0;
            }
            else if (fileExtension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                int[] loopData;
                using (WaveFileReader reader = new WaveFileReader(inputFile))
                {
                    //Get basic info
                    frequency = reader.WaveFormat.SampleRate;
                    channels = reader.WaveFormat.Channels;

                    //Get Loop Data
                    loopData = WavFunctions.ReadSampleChunck(reader);

                    //Get pcm short array
                    byte[] pcmByteData = new byte[reader.Length];
                    reader.Read(pcmByteData, 0, pcmByteData.Length);
                    pcmData = WavFunctions.ConvertByteArrayToShortArray(pcmByteData);
                }

                //Check loop Data
                if (loopData.Length > 0)
                {
                    loopStartValue = SonyVag.GetLoopOffsetForVag((uint)loopData[1]) -1;
                    loopEndValue = SonyVag.GetLoopOffsetForVag((uint)loopData[2])-2;
                }

                isLooped = loopData[0] == 1;
            }

            //Start Encode!!
            if (pcmData != null)
            {
                byte[] vagData;
                if (forceNoLooping)
                {
                    vagData = SonyVag.Encode(pcmData, 0, loopEndValue, false);
                }
                else if (forceLooping)
                {
                    vagData = SonyVag.Encode(pcmData, 0, loopEndValue, true);
                }
                else
                {
                    vagData = SonyVag.Encode(pcmData, loopStartValue, loopEndValue, isLooped);
                }

                //Write File
                SonyVag.WriteVagFile(vagData, outputFile, channels, frequency);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static void ExecuteDecoder(string inputFile, string outputFile)
        {
            //Info that we need for creating the wav file
            if (SonyVag.VagFileIsValid(inputFile, out int sampleRate, out byte[] vagData))
            {
                byte[] pcmData = SonyVag.Decode(vagData);

                //Save file
                IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(pcmData), new WaveFormat(sampleRate, 16, 1));
                try
                {
                    WaveFileWriter.CreateWaveFile(outputFile, provider);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool CheckFileExists(string filePath)
        {
            bool fileExists = false;

            if (File.Exists(filePath))
            {
                fileExists = true;
            }
            else
            {
                Console.WriteLine("ERROR: file not found: " + filePath);
            }

            return fileExists;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static int FlipInt32(int valueToFlip)
        {
            int finalData;

            finalData = (valueToFlip & 0x7F000000) >> (8 * 3) | /* 0x11______ -> 0x______11 */
                        (valueToFlip & 0x00FF0000) >> (8 * 1) | /* 0x__22____ -> 0x____22__ */
                        (valueToFlip & 0x0000FF00) << (8 * 1) | /* 0x____33__ -> 0x__33____ */
                        (valueToFlip & 0x000000FF) << (8 * 3);  /* 0x______44 -> 0x44______ */

            return finalData;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
