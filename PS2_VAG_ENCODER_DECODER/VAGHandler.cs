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

        private struct Vag_chunk
        {
            public sbyte shift_factor;
            public sbyte predict_nr; /* swy: reversed nibbles due to little-endian */
            public byte flag;
            public byte[] s;
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
        public static byte[] VAGEncoder(byte[] pcmData, int numberOfChannels, int bitsPerSample)
        {
            int sampleSize = numberOfChannels * bitsPerSample / 8;

            // size compression is 28 16-bit samples -> 16 bytes
            int numSamples = pcmData.Length / sampleSize;

            for (int pos = 0; pos < numSamples; pos += VAG_SAMPLE_NIBBL)
            {
                for (int ch = 0; ch < numberOfChannels; ch++)
                {


                }
            }
            return null;
        }

        public static byte[] VAGDecoder(byte[] vagData)
        {
            byte[] pcmData;

            using (BinaryReader vagReader = new BinaryReader(new MemoryStream(vagData, false)))
            using (MemoryStream pcmStream = new MemoryStream())
            using (BinaryWriter pcmWriter = new BinaryWriter(pcmStream))
            {
                int hist1 = 0;
                int hist2 = 0;

                while (vagReader.BaseStream.Position < vagData.Length)
                {
                    byte predict_shift = vagReader.ReadByte();

                    //Put the data into the struct
                    Vag_chunk VAGstruct = new Vag_chunk
                    {
                        shift_factor = (sbyte)((predict_shift & 0x0F) >> 0),
                        predict_nr = (sbyte)((predict_shift & 0xF0) >> 4),
                        flag = vagReader.ReadByte(),
                        s = vagReader.ReadBytes(VAG_SAMPLE_BYTES)
                    };

                    int[] unpacked_nibbles = new int[VAG_SAMPLE_NIBBL];

                    if (VAGstruct.flag == (int)VAGFlag.VAGF_END_MARKER_AND_SKIP)
                    {
                        break;
                    }

                    /* swy: unpack one of the 28 'scale' 4-bit nibbles in the 28 bytes; two 'scales' in one byte */
                    for (int j = 0; j < VAG_SAMPLE_BYTES; j++)
                    {
                        short sample_byte = VAGstruct.s[j];

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
                        sbyte predict_nr = (sbyte)Math.Min(VAGstruct.predict_nr, VAG_MAX_LUT_INDX);

                        short sample = (short)((scale >> VAGstruct.shift_factor) + (hist1 * VAGLut[VAGstruct.predict_nr, 0] + hist2 * VAGLut[VAGstruct.predict_nr, 1]) / 64);

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

                GC.Collect();
            }
            return pcmData;
        }

        public static byte[] SplitVAGChannels(string FilePath, bool SplitLeft)
        {
            byte[] ChannelData;
            using (BinaryReader vagReader = new BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            using (MemoryStream vagStream = new MemoryStream())
            using (BinaryWriter vagWritter = new BinaryWriter(vagStream))
            {
                bool Interleaving = SplitLeft;
                
                while (vagReader.BaseStream.Position != vagReader.BaseStream.Length)
                {
                    if (Interleaving)
                    {
                        vagWritter.Write(vagReader.ReadBytes(16));
                    }
                    else
                    {
                        vagReader.ReadBytes(16);
                    }
                    Interleaving = !Interleaving;
                }
                ChannelData = vagStream.ToArray();

                vagWritter.Close();
                vagStream.Close();
                vagReader.Close();
            }
            return ChannelData;
        }
    }
}


