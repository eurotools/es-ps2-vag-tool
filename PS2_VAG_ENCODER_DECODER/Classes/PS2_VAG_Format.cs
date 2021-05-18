using System;
using System.IO;

namespace PS2_VAG_ENCODER_DECODER
{
    internal static class PS2_VAG_Format
    {
        //*===============================================================================================
        //* Definitions and Classes
        //*===============================================================================================
        private static int[,] vag_lut = new int[,]
        {
            {   0,   0 },
            {  60,   0 },
            { 115, -52 },
            {  98, -55 },
            { 122, -60 }
        };

        private enum Vag_flag
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

        //Defines
        private static int VAG_MAX_LUT_INDX = vag_lut.Length - 1;
        private static int VAG_SAMPLE_BYTES = 14;
        private static int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;

        //*===============================================================================================
        //* Encoding / Decoding Functions
        //*===============================================================================================
        public static byte[] EncodeVAG_ADPCM(byte[] pcmData)
        {
            return null;
        }

        public static byte[] DecodeVAG_ADPCM(byte[] vagData)
        {
            byte[] pcmData;

            using (MemoryStream vagStream = new MemoryStream(vagData))
            using (BinaryReader vagReader = new BinaryReader(vagStream))
            using (MemoryStream pcmStream = new MemoryStream())
            using (BinaryWriter pcmWriter = new BinaryWriter(pcmStream))
            {
                int hist1 = 0;
                int hist2 = 0;

                while (vagStream.Position < vagData.Length)
                {
                    //Struct data------
                    byte predict_shift = vagReader.ReadByte();
                    sbyte shift_factor = (sbyte)((predict_shift & 0x0F) >> 0);
                    sbyte predict_nr = (sbyte)((predict_shift & 0xF0) >> 4);
                    byte flag = vagReader.ReadByte();
                    byte[] s = vagReader.ReadBytes(VAG_SAMPLE_BYTES);
                    //-----------------------------

                    int[] unpacked_nibbles = new int[VAG_SAMPLE_NIBBL];

                    if (flag == (int)Vag_flag.VAGF_END_MARKER_AND_SKIP)
                    {
                        break;
                    }

                    /* swy: unpack one of the 28 'scale' 4-bit nibbles in the 28 bytes; two 'scales' in one byte */
                    for (int j = 0; j < VAG_SAMPLE_BYTES; j++)
                    {
                        short sample_byte = s[j];

                        unpacked_nibbles[j * 2] = (sample_byte & 0x0F) >> 0;
                        unpacked_nibbles[j * 2 + 1] = (sample_byte & 0xF0) >> 4;
                    }

                    /* swy: decode each of the 14*2 ADPCM samples in this chunk */
                    for (int j = 0; j < VAG_SAMPLE_NIBBL; j++)
                    {
                        /* swy: turn the signed nibble into a signed int first*/
                        int scale = unpacked_nibbles[j] << 12;
                        if (Convert.ToBoolean(scale & 0x8000))
                        {
                            scale = (int)(scale | 0xFFFF0000);
                        }

                        /* swy: don't overflow the LUT array access; limit the max allowed index */
                        predict_nr = (sbyte)Math.Min(predict_nr, VAG_MAX_LUT_INDX);

                        short sample = (short)((scale >> shift_factor) + (hist1 * vag_lut[predict_nr, 0] + hist2 * vag_lut[predict_nr, 1]) / 64);
                        pcmWriter.Write(sample);

                        /* swy: sliding window with the last two (preceding) decoded samples in the stream/file */
                        hist2 = hist1;
                        hist1 = sample;
                    }
                }

                pcmData = pcmStream.ToArray();

                pcmWriter.Close();
                pcmStream.Close();
                vagReader.Close();
                vagStream.Close();
            }

            return pcmData;
        }
    }
}


