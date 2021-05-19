using System;
using System.Collections.Generic;
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

        private struct VAGChunk
        {
            public sbyte shiftFactor;
            public sbyte predictNR; /* swy: reversed nibbles due to little-endian */
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
        public static byte[] VAGEncoder(short[] pcmData, int numberOfChannels, int bitsPerSample)
        {
            int sampleSize = numberOfChannels * bitsPerSample / 8;

            // size compression is 28 16-bit samples -> 16 bytes
            int numSamples = pcmData.Length / sampleSize;
            double[] factors = new double[sizeof(double)];
            double[] factors2 = new double[sizeof(double)];

            List<VAGChunk> chunks = new List<VAGChunk>();

            for (int pos = 0; pos < numSamples; pos += VAG_SAMPLE_NIBBL)
            {
                for (int ch = 0; ch < numberOfChannels; ch++)
                {
                    VAGChunk chunk = new VAGChunk();

                    // find_predict
                    double min = 1e10;
                    double[,] predictBuffer = new double[5, VAG_SAMPLE_NIBBL];
                    int predict = 0, shift = 0;
                    double[] s1 = new double[5];
                    double[] s2 = new double[5];

                    for (int j = 0; j < 5; j++)
                    {
                        double max = 0;

                        s1[j] = factors[ch * 2];
                        s2[j] = factors[ch * 2 + 1];

                        for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                        {
                            double sample = pcmData[k * numberOfChannels + ch];

                            if (sample > 30719.0)
                            {
                                sample = 30719.0;
                            }

                            if (sample < -30720.0)
                            {
                                sample = -30720.0;
                            }

                            predictBuffer[j, k] = sample - s1[j] * VAGLut[j, 0] - s2[j] * VAGLut[j, 1];

                            if (Math.Abs(predictBuffer[j, k]) > max)
                            {
                                max = Math.Abs(predictBuffer[j, k]);
                            }

                            s2[j] = s1[j];
                            s1[j] = sample;
                        }

                        if (max < min)
                        {
                            min = max;
                            predict = j;
                        }
                    }

                    factors[ch * 2] = s1[predict];
                    factors[ch * 2 + 1] = s2[predict];

                    // find_shift
                    uint shiftMask;

                    for (shift = 0, shiftMask = 0x4000; shift < 12; shift++, shiftMask >>= 1)
                    {
                        if (Convert.ToBoolean(shiftMask & ((int)min + (shiftMask >> 3))))
                        {
                            break;
                        }
                    }
                    // so shift==12 if none found...

                    chunk.shiftFactor = Convert.ToSByte(((predict << 4) & 0xF0) | (shift & 0xF));
                    chunk.flag = Convert.ToByte(numSamples - pos >= 28 ? 0 : 1);

                    // pack
                    short[] outBuf = new short[VAG_SAMPLE_NIBBL];

                    for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                    {
                        double s_double_trans = predictBuffer[predict, k] - factors2[ch * 2] * VAGLut[predict, 0] - factors2[ch * 2 + 1] * VAGLut[predict, 1];
                        // +0x800 for signed conversion??
                        int sample = (int)((((int)Math.Round(s_double_trans) << shift) + 0x800) & 0xFFFFF000);
                        if (sample > 32767)
                        {
                            sample = 32767;
                        }
                        if (sample < -32768)
                        {
                            sample = -32768;
                        }

                        outBuf[k] = (short)(sample >> 12);
                        factors2[ch * 2 + 1] = factors2[ch * 2];
                        factors2[ch * 2] = (sample >> shift) - s_double_trans;
                    }

                    chunk.s = new byte[VAG_SAMPLE_BYTES];

                    for (int k = 0; k < VAG_SAMPLE_BYTES; k++)
                    {
                        chunk.s[k] = Convert.ToByte(((outBuf[k * 2 + 1] << 4) & 0xF0) | (outBuf[k * 2] & 0xF));
                    }

                    chunks.Add(chunk);
                }
            }

            byte[] outgoingData;

            using (MemoryStream memStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memStream))
            {
                foreach (VAGChunk chunk in chunks)
                {
                    writer.Write(chunk.shiftFactor);
                    writer.Write(chunk.predictNR);
                    writer.Write(chunk.flag);
                    writer.Write(chunk.s);
                }
                outgoingData = memStream.ToArray();

                writer.Close();
                memStream.Close();

                GC.Collect();
            }

            return outgoingData;
        }

        private static void find_predict(short[] samples, double[] d_samples, int predict_nr, int shift_factor)
        {
            double [,]buffer = new double[28,5];
            double min = 1e10;
            double [] max = new double[5];
            double ds;
            int min2;
            int shift_mask;
            double _s_1 = 0.0;                            // s[t-1]
            double _s_2 = 0.0;                            // s[t-2]
            double s_0, s_1 = 0, s_2 = 0;

            for (int i = 0; i < 5; i++)
            {
                max[i] = 0.0;
                s_1 = _s_1;
                s_2 = _s_2;
                for (int j = 0; j < VAG_SAMPLE_NIBBL; j++)
                {
                    s_0 = samples[j];                      // s[t-0]
                    if (s_0 > 30719.0)
                    {
                        s_0 = 30719.0;
                    }

                    if (s_0 < -30720.0)
                    {
                        s_0 = -30720.0;
                    }

                    ds = s_0 + s_1 * VAGLut[i,0] + s_2 * VAGLut[i,1];
                    buffer[j,i] = ds;
                    if (Math.Abs(ds) > max[i])
                    {
                        max[i] = Math.Abs(ds);
                    }

                    s_2 = s_1;                                  // new s[t-2]
                    s_1 = s_0;                                  // new s[t-1]
                }

                if (max[i] < min)
                {
                    min = max[i];
                    predict_nr = i;
                }
                if (min <= 7)
                {
                    predict_nr = 0;
                    break;
                }
            }

            // store s[t-2] and s[t-1] in a static variable
            // these than used in the next function call
            _s_1 = s_1;
            _s_2 = s_2;

            for (int i = 0; i < 28; i++)
            {
                d_samples[i] = buffer[i][predict_nr];
            }

            min2 = (int)min;
            shift_mask = 0x4000;
            shift_factor = 0;

            while (shift_factor < 12)
            {
                if (Convert.ToBoolean(shift_mask & (min2 + (shift_mask >> 3))))
                {
                    break;
                } (shift_factor)++;
                shift_mask = shift_mask >> 1;
            }
        }

        private static void pack(double[] d_samples, short[] four_bit, int predict_nr, int shift_factor)
        {
            double ds;
            int di;
            double s_0;
            double s_1 = 0.0;
            double s_2 = 0.0;

            for (int i = 0; i < 28; i++)
            {
                s_0 = d_samples[i] + s_1 * VAGLut[predict_nr,0] + s_2 * VAGLut[predict_nr,1];
                ds = s_0 * (1 << shift_factor);

                di = (int)(((int)ds + 0x800) & 0xfffff000);

                if (di > 32767)
                {
                    di = 32767;
                }

                if (di < -32768)
                {
                    di = -32768;
                }

                four_bit[i] = (short)di;

                di = di >> shift_factor;
                s_2 = s_1;
                s_1 = di - s_0;
            }
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
                    VAGChunk VAGstruct = new VAGChunk
                    {
                        shiftFactor = (sbyte)((predict_shift & 0x0F) >> 0),
                        predictNR = (sbyte)((predict_shift & 0xF0) >> 4),
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
                        sbyte predict_nr = (sbyte)Math.Min(VAGstruct.predictNR, VAG_MAX_LUT_INDX);

                        short sample = (short)((scale >> VAGstruct.shiftFactor) + (hist1 * VAGLut[VAGstruct.predictNR, 0] + hist2 * VAGLut[VAGstruct.predictNR, 1]) / 64);

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

        //*===============================================================================================
        //* Other Functions
        //*===============================================================================================
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
                        vagWritter.Write(vagReader.ReadBytes(128));
                    }
                    else
                    {
                        vagReader.ReadBytes(128);
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


