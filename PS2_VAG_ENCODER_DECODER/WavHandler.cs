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
            short[] PCMDataShortArray;

            using (MemoryStream MSPCMData = new MemoryStream(PCMData))
            {
                using (BinaryReader BReader = new BinaryReader(MSPCMData))
                {
                    PCMDataShortArray = new short[PCMData.Length / 2];

                    //Get data
                    for (int i = 0; i < PCMDataShortArray.Length; i++)
                    {
                        PCMDataShortArray[i] = BReader.ReadInt16();
                    }

                    //Close Reader
                    BReader.Close();
                }
            }

            return PCMDataShortArray;
        }
    }
}
