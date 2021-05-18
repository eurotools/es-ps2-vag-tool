using System;
using System.IO;

namespace PS2_VAG_ENCODER_DECODER
{
    internal static class VAGHandler
    {
        //*===============================================================================================
        //* Definitions and Classes
        //*===============================================================================================
        private static int[,] VAGLut = new int[,]
        {
            {   0,   0 },
            {  60,   0 },
            { 115, -52 },
            {  98, -55 },
            { 122, -60 }
        };

        private enum VAGFlag
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
        private static int VAG_MAX_LUT_INDX = VAGLut.Length - 1;
        private static int VAG_SAMPLE_BYTES = 14;
        private static int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;

        //*===============================================================================================
        //* Encoding / Decoding Functions
        //*===============================================================================================
        public static byte[] VAGEncoder(byte[] pcmData)
        {
            return null;
        }

        public static byte[] VAGDecoder(byte[] vagData)
        {
            byte[] pcmData;

            using (MemoryStream vagMemoryStream = new MemoryStream(vagData))
            using (BinaryReader vagReader = new BinaryReader(vagMemoryStream))
            using (MemoryStream pcmStream = new MemoryStream())
            using (BinaryWriter pcmWriter = new BinaryWriter(pcmStream))
            {
                int hist1 = 0;
                int hist2 = 0;

                while (vagReader.BaseStream.Position < vagData.Length)
                {
                    //Struct data------
                    byte predictShift = vagReader.ReadByte();
                    sbyte shiftFactor = (sbyte)((predictShift & 0x0F) >> 0);
                    sbyte predictNr = (sbyte)((predictShift & 0xF0) >> 4);
                    byte flag = vagReader.ReadByte();
                    byte[] s = vagReader.ReadBytes(VAG_SAMPLE_BYTES);
                    //-----------------------------

                    int[] unpackedNibbles = new int[VAG_SAMPLE_NIBBL];

                    if (flag == (int)VAGFlag.VAGF_END_MARKER_AND_SKIP)
                    {
                        break;
                    }

                    /* swy: unpack one of the 28 'scale' 4-bit nibbles in the 28 bytes; two 'scales' in one byte */
                    for (int j = 0; j < VAG_SAMPLE_BYTES; j++)
                    {
                        short sampleByte = s[j];

                        unpackedNibbles[j * 2] = (sampleByte & 0x0F) >> 0;
                        unpackedNibbles[j * 2 + 1] = (sampleByte & 0xF0) >> 4;
                    }

                    /* swy: decode each of the 14*2 ADPCM samples in this chunk */
                    for (int j = 0; j < VAG_SAMPLE_NIBBL; j++)
                    {
                        /* swy: turn the signed nibble into a signed int first*/
                        int scale = unpackedNibbles[j] << 12;
                        if (Convert.ToBoolean(scale & 0x8000))
                        {
                            scale = (int)(scale | 0xFFFF0000);
                        }

                        /* swy: don't overflow the LUT array access; limit the max allowed index */
                        predictNr = (sbyte)Math.Min(predictNr, VAG_MAX_LUT_INDX);

                        short sample = (short)((scale >> shiftFactor) + (hist1 * VAGLut[predictNr, 0] + hist2 * VAGLut[predictNr, 1]) / 64);

                        pcmWriter.Write(Math.Min(short.MaxValue, Math.Max(sample, short.MinValue)));

                        /* swy: sliding window with the last two (preceding) decoded samples in the stream/file */
                        hist2 = hist1;
                        hist1 = sample;
                    }
                }

                pcmData = pcmStream.ToArray();

                pcmWriter.Close();
                pcmStream.Close();
                vagReader.Close();
                vagMemoryStream.Close();
            }
            return pcmData;
        }
    }
}


