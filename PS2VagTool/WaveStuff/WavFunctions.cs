using NAudio.Wave;
using System;
using System.Linq;

namespace PS2VagTool
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    internal static class WavFunctions
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        internal static short[] ConvertByteArrayToShortArray(byte[] PCMData)
        {
            short[] samplesShort = new short[PCMData.Length / 2];
            WaveBuffer sourceWaveBuffer = new WaveBuffer(PCMData);
            for (int i = 0; i < samplesShort.Length; i++)
            {
                samplesShort[i] = sourceWaveBuffer.ShortBuffer[i];
            }
            return samplesShort;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static int[] ReadSampleChunck(WaveFileReader wavereader)
        {
            int[] loopInfo = new int[3] { 0, 0, 0 };
            RiffChunk smp = wavereader.ExtraChunks.FirstOrDefault(ec => ec.IdentifierAsString == "smpl");
            if (smp != null)
            {
                byte[] chunkData = wavereader.GetChunkData(smp);
                int midiNote = BitConverter.ToInt32(chunkData, 12);
                int numberOfSamples = BitConverter.ToInt32(chunkData, 28);
                int offset = 36;
                for (int n = 0; n < numberOfSamples; n++)
                {
                    //Read Chunck info
                    int cuePointId = BitConverter.ToInt32(chunkData, offset);
                    int type = BitConverter.ToInt32(chunkData, offset + 4); // 0 = loop forward, 1 = alternating loop, 2 = reverse

                    int start = BitConverter.ToInt32(chunkData, offset + 8);
                    int end = BitConverter.ToInt32(chunkData, offset + 12);
                    int fraction = BitConverter.ToInt32(chunkData, offset + 16);
                    int playCount = BitConverter.ToInt32(chunkData, offset + 20);
                    offset += 24;

                    //Save Data
                    loopInfo[0] = 1;
                    loopInfo[1] = start;
                    loopInfo[2] = end;
                }
            }

            return loopInfo;
        }
    }
}
