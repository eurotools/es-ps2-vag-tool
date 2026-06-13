using NAudio.Wave;
using System;
using System.Linq;

namespace PS2VagTool.Audio
{
    internal static class WavLoopReader
    {
        internal static AudioLoopInfo ReadLoop(WaveFileReader reader)
        {
            RiffChunk sampleChunk = reader.ExtraChunks.FirstOrDefault(chunk => chunk.IdentifierAsString == "smpl");
            if (sampleChunk == null)
            {
                return AudioLoopInfo.None;
            }

            byte[] chunkData = reader.GetChunkData(sampleChunk);
            if (chunkData.Length < 36)
            {
                return new AudioLoopInfo(false, 0, 0, null, "WAV smpl chunk is too short and was ignored.");
            }

            int loopCount = BitConverter.ToInt32(chunkData, 28);
            if (loopCount <= 0)
            {
                return new AudioLoopInfo(false, 0, 0, null, "WAV smpl chunk has no sample loops.");
            }

            long totalSampleFrames = reader.Length / reader.WaveFormat.BlockAlign;
            int offset = 36;
            for (int i = 0; i < loopCount; i++)
            {
                if (offset + 24 > chunkData.Length)
                {
                    return new AudioLoopInfo(false, 0, 0, null, "WAV smpl loop data is truncated.");
                }

                int type = BitConverter.ToInt32(chunkData, offset + 4);
                uint start = BitConverter.ToUInt32(chunkData, offset + 8);
                uint end = BitConverter.ToUInt32(chunkData, offset + 12);
                offset += 24;

                if (type != 0)
                {
                    continue;
                }

                return LoopValidator.Validate(start, end, totalSampleFrames, "WAV smpl");
            }

            return new AudioLoopInfo(false, 0, 0, null, "WAV smpl chunk has no supported forward loop.");
        }
    }
}
