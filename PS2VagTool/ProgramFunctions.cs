//-------------------------------------------------------------------------------------------------------------------------------
//  _____   _____ ___   __      __     _____
// |  __ \ / ____|__ \  \ \    / /\   / ____|
// | |__) | (___    ) |  \ \  / /  \ | |  __
// |  ___/ \___ \  / /    \ \/ / /\ \| | |_ |
// | |     ____) |/ /_     \  / ____ \ |__| |
// |_|    |_____/|____|     \/_/    \_\_____|
//
//-------------------------------------------------------------------------------------------------------------------------------
// Audio Conversion Operations
//-------------------------------------------------------------------------------------------------------------------------------
using NAudio.Wave;
using PS2VagTool.Audio;
using PS2VagTool.Vag;
using System;
using System.IO;

namespace PS2VagTool
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    internal static class ProgramFunctions
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool ExecuteEncoder(string inputFile, string outputFile, bool forceNoLooping, bool forceLooping)
        {
            return ExecuteEncoder(inputFile, outputFile, forceNoLooping, forceLooping, false);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool ExecuteEncoder(string inputFile, string outputFile, bool forceNoLooping, bool forceLooping, bool verbose)
        {
            return ExecuteEncoder(inputFile, outputFile, forceNoLooping, forceLooping, verbose, 16);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool ExecuteEncoder(string inputFile, string outputFile, bool forceNoLooping, bool forceLooping, bool verbose, int interleaveSize)
        {
            try
            {
                AudioInputData inputData = AudioInputReader.Read(inputFile);
                if (inputData.Channels > 1)
                {
                    Console.WriteLine("INFO: stereo input will be encoded as independent VAG channels with " + interleaveSize + "-byte interleaving.");
                }

                if (!String.IsNullOrEmpty(inputData.LoopInfo.Warning))
                {
                    Console.WriteLine("WARNING: " + inputData.LoopInfo.Warning);
                }

                VagLoopSettings loopSettings = CreateLoopSettings(inputData, forceNoLooping, forceLooping);
                if (verbose)
                {
                    PrintEncodeInfo(inputFile, outputFile, inputData, loopSettings, forceNoLooping, forceLooping);
                }

                byte[] vagData = SonyVag.Encode(inputData.PcmSamples, inputData.Channels, loopSettings.StartBlock, loopSettings.EndBlock, loopSettings.Enabled, interleaveSize);
                SonyVag.WriteVagFile(vagData, outputFile, inputData.Channels, inputData.SampleRate);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool ExecuteDecoder(string inputFile, string outputFile)
        {
            return ExecuteDecoder(inputFile, outputFile, 16, null);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool ExecuteDecoder(string inputFile, string outputFile, int interleaveSize)
        {
            return ExecuteDecoder(inputFile, outputFile, interleaveSize, null);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool ExecuteDecoder(string inputFile, string outputFile, int interleaveSize, int? sampleFrames)
        {
            try
            {
                if (!SonyVag.VagFileIsValid(inputFile, out int sampleRate, out int channels, out byte[] vagData))
                {
                    return false;
                }

                byte[] pcmData = SonyVag.Decode(vagData, channels, interleaveSize);
                if (sampleFrames.HasValue)
                {
                    long requestedBytes = (long)sampleFrames.Value * channels * sizeof(short);
                    if (requestedBytes > pcmData.Length)
                    {
                        throw new InvalidDataException("--samples exceeds the decoded PCM length.");
                    }

                    Array.Resize(ref pcmData, (int)requestedBytes);
                }

                IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(pcmData), new WaveFormat(sampleRate, 16, channels));
                WaveFileWriter.CreateWaveFile(outputFile, provider);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool CheckFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }

            Console.WriteLine("ERROR: file not found: " + filePath);
            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static int FlipInt32(int valueToFlip)
        {
            return (valueToFlip & 0x7F000000) >> 24 |
                   (valueToFlip & 0x00FF0000) >> 8 |
                   (valueToFlip & 0x0000FF00) << 8 |
                   (valueToFlip & 0x000000FF) << 24;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static VagLoopSettings CreateLoopSettings(AudioInputData inputData, bool forceNoLooping, bool forceLooping)
        {
            if (forceNoLooping)
            {
                return new VagLoopSettings(false, 0, 0);
            }

            if (inputData.LoopInfo.IsLooped)
            {
                return new VagLoopSettings(
                    true,
                    SonyVag.GetLoopBlockIndexForSample(inputData.LoopInfo.StartSample, inputData.Channels),
                    GetLoopEndBlockIndex(inputData.LoopInfo.EndSample, inputData.Channels));
            }

            if (forceLooping)
            {
                return new VagLoopSettings(true, 0, GetLastVagBlockIndex(inputData.SampleFrames));
            }

            return new VagLoopSettings(false, 0, 0);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static uint GetLoopEndBlockIndex(uint loopEndSample, int channels)
        {
            if (loopEndSample == 0)
            {
                return 0;
            }

            return SonyVag.GetLoopBlockIndexForSample(loopEndSample - 1, channels);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static uint GetLastVagBlockIndex(int sampleFrames)
        {
            if (sampleFrames <= 0)
            {
                return 0;
            }

            return (uint)((sampleFrames - 1) / 28);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static void PrintEncodeInfo(string inputFile, string outputFile, AudioInputData inputData, VagLoopSettings loopSettings, bool forceNoLooping, bool forceLooping)
        {
            Console.WriteLine("Input: " + inputFile);
            Console.WriteLine("Output: " + outputFile);
            Console.WriteLine("Format: PCM 16-bit, " + inputData.SampleRate + " Hz, " + inputData.Channels + " channel(s)");
            Console.WriteLine("Samples: " + inputData.SampleFrames);

            if (forceNoLooping)
            {
                Console.WriteLine("Loop: disabled by -1");
            }
            else if (forceLooping && !inputData.LoopInfo.IsLooped)
            {
                Console.WriteLine("Loop: forced by -L");
                Console.WriteLine("VAG loop blocks: " + loopSettings.StartBlock + " -> " + loopSettings.EndBlock);
            }
            else if (inputData.LoopInfo.IsLooped)
            {
                Console.WriteLine("Loop: " + inputData.LoopInfo.Source + " samples " + inputData.LoopInfo.StartSample + " -> " + inputData.LoopInfo.EndSample);
                Console.WriteLine("VAG loop blocks: " + loopSettings.StartBlock + " -> " + loopSettings.EndBlock);
                Console.WriteLine("VAG loop byte offsets: " + SonyVag.GetLoopByteOffsetForSample(inputData.LoopInfo.StartSample, inputData.Channels) + " -> " + SonyVag.GetLoopByteOffsetForSample(inputData.LoopInfo.EndSample, inputData.Channels));
            }
            else
            {
                Console.WriteLine("Loop: none");
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        private sealed class VagLoopSettings
        {
            internal VagLoopSettings(bool enabled, uint startBlock, uint endBlock)
            {
                Enabled = enabled;
                StartBlock = startBlock;
                EndBlock = endBlock;
            }

            internal bool Enabled { get; private set; }
            internal uint StartBlock { get; private set; }
            internal uint EndBlock { get; private set; }
        }
    }
}
