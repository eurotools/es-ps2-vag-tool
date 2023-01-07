using System;
using System.IO;

namespace PS2VagTool.Vag_Functions
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
        public static byte[] Decode(byte[] vagData)
        {
            byte[] pcmData;

            using (BinaryReader VagReader = new BinaryReader(new MemoryStream(vagData, false)))
            using (MemoryStream PCMStream = new MemoryStream())
            using (BinaryWriter PCMWriter = new BinaryWriter(PCMStream))
            {
                double hist_1 = 0.0, hist_2 = 0.0;

                //Skip header
                VagReader.BaseStream.Seek(16, SeekOrigin.Begin);

                //Start decoding
                while (VagReader.BaseStream.Position < VagReader.BaseStream.Length)
                {
                    //Read chunk data
                    byte DecodingCoefficent = VagReader.ReadByte();
                    VAGChunk vc = new VAGChunk
                    {
                        shift = (sbyte)(DecodingCoefficent & 0xF),
                        predict = (sbyte)((DecodingCoefficent & 0xF0) >> 4),
                        flags = VagReader.ReadByte(),
                        sample = VagReader.ReadBytes(14)
                    };

                    if (vc.flags == (byte)VAGFlag.VAGF_PLAYBACK_END)
                    {
                        break;
                    }
                    else if(vc.flags == (byte)VAGFlag.VAGF_LOOP_START)
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

                            /* swy: don't overflow the LUT array access; limit the max allowed index */
                            sbyte predict = Math.Min(vc.predict, (sbyte)(VagLutDecoder.GetLength(0) - 1));

                            double sample = (s >> vc.shift) + hist_1 * VagLutDecoder[predict, 0] + hist_2 * VagLutDecoder[predict, 1];
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
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
