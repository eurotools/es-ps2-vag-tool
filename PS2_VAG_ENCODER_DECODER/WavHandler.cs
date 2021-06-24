using NAudio.Wave;
using System.IO;

namespace PS2_VAG_ENCODER_DECODER
{
    public class WavHandler
    {
        internal static void CreateWavFile(int frequency, int bitsPerChannel, int numberOfChannels, byte[] PCMData, string filePath)
        {
            WaveFormat waveFormat = new WaveFormat(frequency, bitsPerChannel, numberOfChannels);
            WaveStream sourceStream = new RawSourceWaveStream(new MemoryStream(PCMData), waveFormat);
            WaveFileWriter.CreateWaveFile(filePath, sourceStream);
        }

        internal static void CreateWavFileStereo(int frequency, int bitsPerChannel, byte[] PCMDataLeft, byte[] PCMDataRight, string filePath)
        {
            MemoryStream AudioSampleLeft = new MemoryStream(PCMDataLeft);
            IWaveProvider providerLeft = new RawSourceWaveStream(AudioSampleLeft, new WaveFormat(frequency, bitsPerChannel, 1));

            MemoryStream AudioSampleRight = new MemoryStream(PCMDataRight);
            IWaveProvider providerRight = new RawSourceWaveStream(AudioSampleRight, new WaveFormat(frequency, bitsPerChannel, 1));

            MultiplexingWaveProvider waveProvider = new MultiplexingWaveProvider(new IWaveProvider[] { providerLeft, providerRight }, 2);
            WaveFileWriter.CreateWaveFile(filePath, waveProvider);
        }

        internal static byte[] GetPCMDataFromWav(string FilePath)
        {
            byte[] OutputPCMData;

            using (WaveFileReader audioReader = new WaveFileReader(FilePath))
            {
                //Get PCM Data
                OutputPCMData = new byte[audioReader.Length];
                audioReader.Read(OutputPCMData, 0, (int)audioReader.Length);
                audioReader.Close();
            }

            return OutputPCMData;
        }

        internal static short[] ConvertPCMDataToShortArray(byte[] PCMData)
        {
            short[] samplesShort = new short[PCMData.Length / 2];
            WaveBuffer sourceWaveBuffer = new WaveBuffer(PCMData);
            for (int i = 0; i < samplesShort.Length; i++)
            {
                samplesShort[i] = sourceWaveBuffer.ShortBuffer[i];
            }
            return samplesShort;
        }

        internal static short[] SplitWavChannels(short[] inputChannel, bool leftChannel)
        {
            short[] channelData = new short[inputChannel.Length / 2];
            int channelDataIndex = 0;

            for (int i = 0; i < inputChannel.Length; i++)
            {
                if (leftChannel)
                {
                    channelData[channelDataIndex] = inputChannel[i];
                    channelDataIndex++;
                }
                leftChannel = !leftChannel;
            }

            return channelData;
        }
    }
}
