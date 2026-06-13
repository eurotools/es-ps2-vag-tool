using NAudio.Wave;
using System;
using System.IO;

namespace PS2VagTool.Audio
{
    internal static class AudioInputReader
    {
        internal static AudioInputData Read(string inputFile)
        {
            string extension = Path.GetExtension(inputFile);
            if (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return ReadWav(inputFile);
            }

            if (extension.Equals(".aif", StringComparison.OrdinalIgnoreCase) || extension.Equals(".aiff", StringComparison.OrdinalIgnoreCase))
            {
                return ReadAiff(inputFile);
            }

            throw new InvalidOperationException("unsupported input format: " + extension + ". Use WAV, AIF or AIFF.");
        }

        private static AudioInputData ReadWav(string inputFile)
        {
            using (WaveFileReader reader = new WaveFileReader(inputFile))
            {
                ValidateFormat(reader.WaveFormat);
                AudioLoopInfo loopInfo = WavLoopReader.ReadLoop(reader);
                short[] samples = PcmUtilities.ConvertBytesToInt16Samples(PcmUtilities.ReadAllBytes(reader));
                return new AudioInputData(samples, reader.WaveFormat.SampleRate, reader.WaveFormat.Channels, loopInfo);
            }
        }

        private static AudioInputData ReadAiff(string inputFile)
        {
            short[] samples;
            int sampleRate;
            int channels;

            using (AiffFileReader reader = new AiffFileReader(inputFile))
            {
                ValidateFormat(reader.WaveFormat);
                sampleRate = reader.WaveFormat.SampleRate;
                channels = reader.WaveFormat.Channels;
                samples = PcmUtilities.ConvertBytesToInt16Samples(PcmUtilities.ReadAllBytes(reader));
            }

            AudioLoopInfo loopInfo;
            using (FileStream stream = File.OpenRead(inputFile))
            {
                loopInfo = AiffLoopReader.ReadLoop(stream, samples.Length / Math.Max(channels, 1));
            }

            return new AudioInputData(samples, sampleRate, channels, loopInfo);
        }

        private static void ValidateFormat(WaveFormat waveFormat)
        {
            if (!PcmUtilities.IsSupportedPcmFormat(waveFormat, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}
