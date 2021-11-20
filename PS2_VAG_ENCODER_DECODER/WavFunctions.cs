using NAudio.Wave;
using System;
using System.IO;

namespace PS2_VAG_ENCODER_DECODER
{
    public class WavFunctions
    {
        public static short[][] SplitChannels(short[] inout, int channels)
        {
            short[][] tempbuf = new short[2][];
            int length = inout.Length / 2;
            for (int c = 0; c < channels; c++)
            {
                tempbuf[c] = new short[length];
            }
            for (int ix = 0, i = 0; ix < length; ix++)
            {
                tempbuf[0][ix] = inout[i++];
                tempbuf[1][ix] = inout[i++];
            }
            return tempbuf;
        }

        internal static void CreateStereoWavFile(string filePath, byte[] pcmDataLeft, byte[] pcmDataRight, int frequency)
        {
            MemoryStream AudioSample = new MemoryStream(pcmDataLeft);
            IWaveProvider providerLeft = new RawSourceWaveStream(AudioSample, new WaveFormat(frequency, 16, 1));
            AudioSample = new MemoryStream(pcmDataRight);
            IWaveProvider providerRight = new RawSourceWaveStream(AudioSample, new WaveFormat(frequency, 16, 1));
            MultiplexingWaveProvider waveProvider = new MultiplexingWaveProvider(new IWaveProvider[] { providerLeft, providerRight }, 2);
            WaveFileWriter.CreateWaveFile(filePath, waveProvider);
        }

        internal static void CreateMonoWavFile(string filePath, byte[] pcmData, int frequency, int bits)
        {
            IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(pcmData), new WaveFormat(frequency, bits, 1));
            WaveFileWriter.CreateWaveFile(filePath, provider);
        }

        internal static byte[] ShortArrayToByteArray(short[] inputArray)
        {
            byte[] byteArray = new byte[inputArray.Length * 2];
            Buffer.BlockCopy(inputArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
    }
}
