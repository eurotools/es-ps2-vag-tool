//-------------------------------------------------------------------------------------------------------------------------------
//  _____   _____ ___   __      __     _____
// |  __ \ / ____|__ \  \ \    / /\   / ____|
// | |__) | (___    ) |  \ \  / /  \ | |  __
// |  ___/ \___ \  / /    \ \/ / /\ \| | |_ |
// | |     ____) |/ /_     \  / ____ \ |__| |
// |_|    |_____/|____|     \/_/    \_\_____|
//
//-------------------------------------------------------------------------------------------------------------------------------
// VAG Decoder
//-------------------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;

namespace PS2VagTool.Vag
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    public static partial class SonyVag
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        private static readonly double[,] VagLutDecoder = new double[,]
        {
            {0.0, 0.0},
            {60.0 / 64.0, 0.0},
            {115.0 / 64.0, -52.0 / 64.0},
            {98.0 / 64.0, -55.0 / 64.0},
            {122.0 / 64.0, -60.0 / 64.0}
        };

        //-------------------------------------------------------------------------------------------------------------------------------
        public static byte[] Decode(byte[] vagData, int channels)
        {
            return Decode(vagData, channels, 16);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        public static byte[] Decode(byte[] vagData, int channels, int interleaveSize)
        {
            if (channels <= 1)
            {
                return Decode(vagData);
            }

            if (channels != 2)
            {
                throw new ArgumentException("Only mono and stereo VAG decoding is supported.");
            }

            byte[] leftVag;
            byte[] rightVag;
            DeinterleaveVagBlocks(vagData, interleaveSize, out leftVag, out rightVag);

            byte[] leftPcm = Decode(leftVag);
            byte[] rightPcm = Decode(rightVag);
            return InterleavePcm16(leftPcm, rightPcm);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        public static byte[] Decode(byte[] vagData)
        {
            byte[] pcmData;

            using (BinaryReader VagReader = new BinaryReader(new MemoryStream(vagData, false)))
            using (MemoryStream PCMStream = new MemoryStream())
            using (BinaryWriter PCMWriter = new BinaryWriter(PCMStream))
            {
                double hist_1 = 0.0, hist_2 = 0.0;

                //Start decoding
                while (VagReader.BaseStream.Position < VagReader.BaseStream.Length)
                {
                    if (VagReader.BaseStream.Length - VagReader.BaseStream.Position < 16)
                    {
                        throw new InvalidDataException("Truncated VAG ADPCM block.");
                    }

                    //Read chunk data
                    byte DecodingCoefficent = VagReader.ReadByte();
                    VAGChunk vc = new VAGChunk
                    {
                        shift = (sbyte)(DecodingCoefficent & 0xF),
                        predict = (sbyte)((DecodingCoefficent & 0xF0) >> 4),
                        flags = VagReader.ReadByte(),
                        sample = VagReader.ReadBytes(14)
                    };

                    if (vc.predict < 0 || vc.predict >= VagLutDecoder.GetLength(0))
                    {
                        throw new InvalidDataException("Invalid VAG predictor index: " + vc.predict + ".");
                    }
                    if (vc.shift < 0 || vc.shift > 12)
                    {
                        throw new InvalidDataException("Invalid VAG shift value: " + vc.shift + ".");
                    }

                    if (vc.flags == (byte)VAGFlag.VAGF_PLAYBACK_END)
                    {
                        break;
                    }
                    else if (vc.flags == (byte)VAGFlag.VAGF_LOOP_START)
                    {
                        var sample = PCMStream.Length / 2;
                    }
                    else
                    {
                        int[] samples = new int[VAG_SAMPLE_NIBBL];

                        // expand 4bit -> 8bit
                        for (int j = 0; j < VAG_SAMPLE_BYTES; j++)
                        {
                            samples[j * 2] = vc.sample[j] & 0xF;
                            samples[j * 2 + 1] = (vc.sample[j] & 0xF0) >> 4;
                        }

                        //Decode samples
                        for (int j = 0; j < VAG_SAMPLE_NIBBL; j++)
                        {
                            // shift 4 bits to top range of int16_t
                            int s = samples[j] << 12;
                            if ((s & 0x8000) != 0)
                            {
                                s = (int)(s | 0xFFFF0000);
                            }

                            double sample = (s >> vc.shift) + hist_1 * VagLutDecoder[vc.predict, 0] + hist_2 * VagLutDecoder[vc.predict, 1];
                            hist_2 = hist_1;
                            hist_1 = sample;

                            PCMWriter.Write((short)(Math.Min(short.MaxValue, Math.Max(sample, short.MinValue))));
                        }
                    }
                }
                pcmData = PCMStream.ToArray();

                PCMWriter.Close();
                PCMStream.Close();
                VagReader.Close();
            }

            return pcmData;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static void DeinterleaveVagBlocks(byte[] stereoVagData, int interleaveSize, out byte[] leftVag, out byte[] rightVag)
        {
            if (interleaveSize <= 0 || interleaveSize > 0x100000 || (interleaveSize % 16) != 0)
            {
                throw new ArgumentOutOfRangeException("interleaveSize", "VAG interleave size must be a positive multiple of 16.");
            }

            int interleavePairSize = interleaveSize * 2;
            if ((stereoVagData.Length % interleavePairSize) != 0)
            {
                throw new InvalidDataException("Stereo VAG data is not aligned to left/right block pairs.");
            }

            int interleavePairs = stereoVagData.Length / interleavePairSize;
            leftVag = new byte[interleavePairs * interleaveSize];
            rightVag = new byte[interleavePairs * interleaveSize];

            for (int block = 0; block < interleavePairs; block++)
            {
                int inputOffset = block * interleavePairSize;
                int outputOffset = block * interleaveSize;
                Buffer.BlockCopy(stereoVagData, inputOffset, leftVag, outputOffset, interleaveSize);
                Buffer.BlockCopy(stereoVagData, inputOffset + interleaveSize, rightVag, outputOffset, interleaveSize);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static byte[] InterleavePcm16(byte[] leftPcm, byte[] rightPcm)
        {
            int sampleBytes = Math.Min(leftPcm.Length, rightPcm.Length);
            sampleBytes -= sampleBytes % 2;

            byte[] stereoPcm = new byte[sampleBytes * 2];
            for (int sourceOffset = 0, targetOffset = 0; sourceOffset < sampleBytes; sourceOffset += 2, targetOffset += 4)
            {
                stereoPcm[targetOffset] = leftPcm[sourceOffset];
                stereoPcm[targetOffset + 1] = leftPcm[sourceOffset + 1];
                stereoPcm[targetOffset + 2] = rightPcm[sourceOffset];
                stereoPcm[targetOffset + 3] = rightPcm[sourceOffset + 1];
            }

            return stereoPcm;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
