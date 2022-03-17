using NAudio.Wave;
using PS2VagTool.Vag_Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            string fileExtension = Path.GetExtension(inputFile);
            if (fileExtension.Equals(".aif", StringComparison.OrdinalIgnoreCase))
            {
                //Read file data
                short[] pcmData;
                byte[] vagData;
                int frequency, channels;
                using (AiffFileReader reader = new AiffFileReader(inputFile))
                {
                    //Get pcm short array
                    byte[] pcmByteData = new byte[reader.Length];
                    reader.Read(pcmByteData, 0, pcmByteData.Length);
                    pcmData = WavFunctions.ConvertByteArrayToShortArray(pcmByteData);
                    frequency = reader.WaveFormat.SampleRate;
                    channels = reader.WaveFormat.Channels;
                }

                //Get markers
                List<MarkerChunkData> markers = new List<MarkerChunkData>();
                AiffFileChunksReader.ReadAiffHeader(File.OpenRead(inputFile), markers);
                uint loopOffsetValue = 0;
                if (markers.Count > 0)
                {
                    loopOffsetValue = SonyVag.GetLoopOffsetForVag(markers[0].position) - 1;
                }

                //Start Encode!!
                if (forceNoLooping)
                {
                    vagData = SonyVag.Encode(pcmData, 0, false);
                }
                else if (forceLooping)
                {
                    vagData = SonyVag.Encode(pcmData, 0, true);
                }
                else
                {
                    vagData = SonyVag.Encode(pcmData, loopOffsetValue, markers.Count > 0);
                }

                //Write File
                SonyVag.WriteVagFile(vagData, outputFile, channels, frequency);
            }
            else if (fileExtension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                short[] pcmData;
                byte[] vagData;
                int[] loopData;
                int frequency, channels;
                //Get File Data
                using (WaveFileReader reader = new WaveFileReader(inputFile))
                {
                    //Get Loop Data
                    loopData = WavFunctions.ReadSampleChunck(reader);

                    //Get pcm short array
                    byte[] pcmByteData = new byte[reader.Length];
                    reader.Read(pcmByteData, 0, pcmByteData.Length);
                    pcmData = WavFunctions.ConvertByteArrayToShortArray(pcmByteData);
                    frequency = reader.WaveFormat.SampleRate;
                    channels = reader.WaveFormat.Channels;
                }

                //Check loop Data
                uint loopOffsetValue = 0;
                if (loopData.Length > 0)
                {
                    loopOffsetValue = SonyVag.GetLoopOffsetForVag((uint)loopData[1]);
                }

                //Start Encode!!
                if (forceNoLooping)
                {
                    vagData = SonyVag.Encode(pcmData, 0, false);
                }
                else if (forceLooping)
                {
                    vagData = SonyVag.Encode(pcmData, 0, true);
                }
                else
                {
                    vagData = SonyVag.Encode(pcmData, loopOffsetValue, loopData[0] == 1);
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
                WaveFileWriter.CreateWaveFile(outputFile, provider);
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
        internal static bool CheckDirectoryExists(string directoryPathToCheck)
        {
            bool directoryExists = false;

            if (Directory.Exists(Path.GetDirectoryName(directoryPathToCheck)))
            {
                directoryExists = true;
            }
            else
            {
                Console.WriteLine("ERROR: output directory not found: " + directoryPathToCheck);
            }

            return directoryExists;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
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
