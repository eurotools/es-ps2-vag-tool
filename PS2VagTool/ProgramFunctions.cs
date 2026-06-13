using NAudio.Wave;
using PS2VagTool.Audio;
using PS2VagTool.Vag;
using System;
using System.IO;

namespace PS2VagTool
{
    internal static class ProgramFunctions
    {
        internal static void ExecuteEncoder(string inputFile, string outputFile, bool forceNoLooping, bool forceLooping)
        {
            ExecuteEncoder(inputFile, outputFile, forceNoLooping, forceLooping, false);
        }

        internal static void ExecuteEncoder(string inputFile, string outputFile, bool forceNoLooping, bool forceLooping, bool verbose)
        {
            try
            {
                AudioInputData inputData = AudioInputReader.Read(inputFile);
                if (inputData.Channels > 1)
                {
                    Console.WriteLine("INFO: stereo input will be encoded as independent interleaved VAG channels.");
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

                byte[] vagData = SonyVag.Encode(inputData.PcmSamples, inputData.Channels, loopSettings.StartBlock, loopSettings.EndBlock, loopSettings.Enabled);
                SonyVag.WriteVagFile(vagData, outputFile, inputData.Channels, inputData.SampleRate);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        internal static void ExecuteDecoder(string inputFile, string outputFile)
        {
            if (SonyVag.VagFileIsValid(inputFile, out int sampleRate, out int channels, out byte[] vagData))
            {
                byte[] pcmData = SonyVag.Decode(vagData, channels);

                IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(pcmData), new WaveFormat(sampleRate, 16, channels));
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

        internal static bool CheckFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }

            Console.WriteLine("ERROR: file not found: " + filePath);
            return false;
        }

        internal static int FlipInt32(int valueToFlip)
        {
            return (valueToFlip & 0x7F000000) >> 24 |
                   (valueToFlip & 0x00FF0000) >> 8 |
                   (valueToFlip & 0x0000FF00) << 8 |
                   (valueToFlip & 0x000000FF) << 24;
        }

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

        private static uint GetLoopEndBlockIndex(uint loopEndSample, int channels)
        {
            if (loopEndSample == 0)
            {
                return 0;
            }

            return SonyVag.GetLoopBlockIndexForSample(loopEndSample - 1, channels);
        }

        private static uint GetLastVagBlockIndex(int sampleFrames)
        {
            if (sampleFrames <= 0)
            {
                return 0;
            }

            return (uint)((sampleFrames - 1) / 28);
        }

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
