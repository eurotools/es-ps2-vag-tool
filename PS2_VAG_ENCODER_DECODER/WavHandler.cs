using System.IO;
using System.Text;

namespace PS2_VAG_ENCODER_DECODER
{
    public class WavHandler
    {
        // TODO: Convert some of this to a struct
        internal static void CreateWavFile(
            int frequency,
            int bitsPerChannel,
            int numberOfChannels,
            int interleave,
            byte[] pcmData,
            string filePath)
        {
            using (FileStream wavFile = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter binaryWriter = new BinaryWriter(wavFile))
            {
                //Write WAV Header
                binaryWriter.Write(Encoding.UTF8.GetBytes("RIFF")); //Chunk ID
                binaryWriter.Write((uint)(36 + pcmData.Length)); //Chunk Size
                binaryWriter.Write(Encoding.UTF8.GetBytes("WAVE")); //Format
                binaryWriter.Write(Encoding.UTF8.GetBytes("fmt ")); //Subchunk1 ID
                binaryWriter.Write((uint)16); //Subchunk1 Size
                binaryWriter.Write((ushort)1); //Audio Format
                binaryWriter.Write((ushort)numberOfChannels); //Num Channels
                binaryWriter.Write((uint)(frequency)); //Sample Rate
                binaryWriter.Write((uint)((frequency * numberOfChannels * bitsPerChannel) / 8)); //Byte Rate
                binaryWriter.Write((ushort)((numberOfChannels * bitsPerChannel) / 8)); //Block Align
                binaryWriter.Write((ushort)(bitsPerChannel)); //Bits Per Sample
                binaryWriter.Write(Encoding.UTF8.GetBytes("data")); //Subchunk2 ID
                binaryWriter.Write((uint)pcmData.Length); //Subchunk2 Size

                var rightChannelIndex = 0;
                var leftChannelIndex = 0;
                var stereoInterleaving = interleave != 0;

                //Write PCM Data
                for (int i = 0; i < pcmData.Length; i++)
                {
                    // TODO: Interleaving, not correct here at all but template
                    //if (stereoInterleaving)
                    //{
                    //    binaryWriter.Write(pcmData[leftChannelIndex]);
                    //    leftChannelIndex++;
                    //}
                    //else
                    //{
                    //    binaryWriter.Write(pcmData[rightChannelIndex]);
                    //    rightChannelIndex++;
                    //}
                    stereoInterleaving = !stereoInterleaving;
                }

                //Close Writter
                binaryWriter.Close();
                //Close File
                wavFile.Close();
            }
        }
    }
}
