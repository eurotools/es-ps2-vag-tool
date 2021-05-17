using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PS2_VAG_ENCODER_DECODER
{
    internal static class PS2_VAG_Format
    {
        //*===============================================================================================
        //* Definitions and Classes
        //*===============================================================================================
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
            public byte shift_factor;
            public byte predict_nr; /* swy: reversed nibbles due to little-endian */
            public byte flag;
            public byte[] s;
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

        //Defines
        private static int VAG_MAX_LUT_INDX = vag_lut.Length / 2 - 1;
        private static int VAG_SAMPLE_BYTES = 14;
        private static int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;


        //*===============================================================================================
        //* Encoding / Decoding Functions
        //*===============================================================================================
        public static byte[] DecodeVAG_ADPCM(byte[] vagFileData, int numChannels)
        {
            uint inChnk = Convert.ToUInt32(vagFileData.Length / Marshal.SizeOf(typeof(vag_chunk)));
            uint outSze = (uint)(inChnk * (VAG_SAMPLE_NIBBL * sizeof(short)));

            byte[] outp;

            using (MemoryStream decodedData = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(decodedData))
            {
                int hist1 = 0;
                int hist2 = 0;
                vag_chunk cur_chunk = new vag_chunk();
                cur_chunk.s = new byte[VAG_SAMPLE_BYTES];

                /* swy: loop for each 16-byte chunk */
                for (int i = 0; i < vagFileData.Length; i++)
                {
                    int[] unpacked_nibbles = new int[VAG_SAMPLE_NIBBL];

                    if (cur_chunk.flag == (int)vag_flag.VAGF_END_MARKER_AND_SKIP)
                    {
                        break;
                    }

                    /* swy: unpack one of the 28 'scale' 4-bit nibbles in the 28 bytes; two 'scales' in one byte */
                    for (int j = 0, nib = 0; j < VAG_SAMPLE_BYTES; j++)
                    {
                        short sample_byte = cur_chunk.s[j];

                        unpacked_nibbles[nib++] = (sample_byte & 0x0F) >> 0;
                        unpacked_nibbles[nib++] = (sample_byte & 0xF0) >> 4;
                    }

                    /* swy: decode each of the 14*2 ADPCM samples in this chunk */
                    for (int j = 0; j < VAG_SAMPLE_NIBBL; j++)
                    {
                        /* swy: same as multiplying it by 4096; turn the signed nibble into a signed int first, though */
                        int scale = SignExtended(unpacked_nibbles[j], 4) << 12;

                        /* swy: don't overflow the LUT array access; limit the max allowed index */
                        byte predict_nr = (byte)Math.Min(i, VAG_MAX_LUT_INDX);

                        int sample = (scale >> cur_chunk.shift_factor) + hist1 * vag_lut[predict_nr, 0] + hist2 * vag_lut[predict_nr, 1];
                        binaryWriter.Write(sample);

                        /* swy: sliding window with the last two (preceding) decoded samples in the stream/file */
                        hist2 = hist1;
                        hist1 = sample;
                    }

                    if (cur_chunk.flag == (int)vag_flag.VAGF_END_MARKER_AND_DEC)
                    {
                        break;
                    }
                }
                outp = decodedData.ToArray();
            }
            if (outSze == outp.Length)
                Debug.WriteLine("Matched!!");
            return outp;
        }

        private static int SignExtended(int val, uint bits)
        {
            int shift = Convert.ToInt32(8 * sizeof(int) - bits);
            var v = new UnionSignedUnsigned() { u = (uint)val << shift };
            return v.s >> shift;
        }

        private class UnionSignedUnsigned
        {
            public uint u;
            public int s;
        }
    }
}
