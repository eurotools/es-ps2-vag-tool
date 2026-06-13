using NAudio.Wave;
using System;

namespace PS2VagTool.Audio
{
    internal static class PcmUtilities
    {
        internal static short[] ConvertBytesToInt16Samples(byte[] pcmData)
        {
            short[] samples = new short[pcmData.Length / 2];
            Buffer.BlockCopy(pcmData, 0, samples, 0, samples.Length * sizeof(short));

            return samples;
        }

        internal static byte[] ReadAllBytes(WaveStream reader)
        {
            byte[] buffer = new byte[reader.Length];
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = reader.Read(buffer, totalRead, buffer.Length - totalRead);
                if (bytesRead == 0)
                {
                    break;
                }

                totalRead += bytesRead;
            }

            if (totalRead != buffer.Length)
            {
                Array.Resize(ref buffer, totalRead);
            }

            return buffer;
        }

        internal static bool IsSupportedPcmFormat(WaveFormat waveFormat, out string errorMessage)
        {
            errorMessage = null;

            if (waveFormat == null)
            {
                errorMessage = "unable to read audio format.";
                return false;
            }

            if (waveFormat.SampleRate <= 0)
            {
                errorMessage = "invalid sample rate: " + waveFormat.SampleRate + ".";
                return false;
            }

            if (waveFormat.Encoding != WaveFormatEncoding.Pcm && waveFormat.Encoding != WaveFormatEncoding.Extensible)
            {
                errorMessage = "only PCM WAV/AIFF files are supported. Current encoding: " + waveFormat.Encoding + ".";
                return false;
            }

            if (waveFormat.BitsPerSample != 16)
            {
                errorMessage = "only 16-bit PCM WAV/AIFF files are supported. Current bit depth: " + waveFormat.BitsPerSample + ".";
                return false;
            }

            if (waveFormat.Channels < 1 || waveFormat.Channels > 2)
            {
                errorMessage = "only mono and stereo files are supported. Current channels: " + waveFormat.Channels + ".";
                return false;
            }

            return true;
        }
    }
}
