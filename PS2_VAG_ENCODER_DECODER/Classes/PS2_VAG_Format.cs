using System;
using System.IO;

namespace PS2_VAG_ENCODER_DECODER
{
    internal static class PS2_VAG_Format
    {
        private static int[,] vag_lut = new int[5, 2]
        {
            {   0,   0 },
            {  60,   0 },
            { 115, -52 },
            {  98, -55 },
            { 122, -60 }
        };

        private struct vag_chunk
        {
            byte shift_factor;
            byte predict_nr; /* swy: reversed nibbles due to little-endian */
            byte flag;
            byte[] s;
        };

        private enum vag_flag
        {
            VAGF_NOTHING = 0, /* Nothing*/
            VAGF_END_MARKER_AND_DEC = 1, /* End marker + decode*/
            VAGF_LOOP_REGION = 2, /* Loop region*/
            VAGF_LOOP_END = 3, /* Loop end*/
            VAGF_START_MARKER = 4, /* Start marker*/
            VAGF_UNK = 5, /* ?*/
            VAGF_LOOP_START = 6, /* Loop start*/
            VAGF_END_MARKER_AND_SKIP = 7  /* End marker + don't decode */
        };

        public static byte[] DecodeVAG_ADPCM(byte[] VagFileData, int NumSamples)
        {
            byte[] outp;

            using (MemoryStream DecodedData = new MemoryStream())
            {
                using (BinaryWriter BWriter = new BinaryWriter(DecodedData))
                {
                    int[] unpacked_nibbles = new int[NumSamples * 2];

                    //Expand 4bit -> 8bit
                    for (int i = 0; i < NumSamples; i++)
                    {
                        short sample_byte = VagFileData[i];

                        unpacked_nibbles[i * 2] = (sample_byte & 0x0F) >> 0;
                        unpacked_nibbles[i * 2 + 1] = (sample_byte & 0xF0) >> 4;
                    }

                    for (int j = 0; j < 28; j++)
                    {
                        int s = unpacked_nibbles[j] << 12;

                        if (Convert.ToBoolean(s & 0x8000))
                        {
                            s = (int)(s | 0xFFFF0000);
                        }
                    }
                }
                outp = DecodedData.ToArray();
            }
            return outp;
        }
    }
}
