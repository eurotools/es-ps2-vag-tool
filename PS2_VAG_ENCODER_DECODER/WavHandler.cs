using NAudio.Wave;
using System.IO;
using System.Text;

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
    }
}
