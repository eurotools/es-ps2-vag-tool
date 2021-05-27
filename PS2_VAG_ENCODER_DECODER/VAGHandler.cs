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
        private static double[,] VAGLut = new double[,]
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

        private struct VAGStruct
        {
            public byte predict_shift;	// upper 4 bits = predict, lower 4 bits = shift
            public byte flag; // 0 or 7 (end)
            public byte[] s;
        };

        private enum VAGFlag
        {
            VAGF_NOTHING = 0, /* Nothing*/
            VAGF_END_MARKER_AND_DEC = 1, /* last block of the file*/
            VAGF_LOOP_REGION = 2, /* Loop region*/
            VAGF_LOOP_END = 3, /* ending block of the loop*/
            VAGF_START_MARKER = 4, /* Start marker*/
            VAGF_UNK = 5, /* ?*/
            VAGF_LOOP_START = 6, /* starting block of the loop*/
            VAGF_END_MARKER_AND_SKIP = 7  /* playback ending position */
        };

        //Defines
        private static int VAG_MAX_LUT_INDX = VAGLut.Length - 1;
        private static int VAG_SAMPLE_BYTES = 14;
        private static int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;

        //*===============================================================================================
        //* Encoding / Decoding Functions
        //*===============================================================================================
        public static byte[] VAGEncoder(short[] pcmData, int bitDepth)
        {
            byte[] vagDat;
            const int numChannels = 1;

            // size compression is 28 16-bit samples -> 16 bytes
            int s2size = pcmData.Length * numChannels * bitDepth / 8;
            int sampleSize = (numChannels * bitDepth) / 8;

            uint numSamples = ((uint)(s2size / sampleSize));

            //int wavBufSize = 28 * (bitDepth / 8) * numChannels;
            int wavBufSize = 28;
            short[] wavBuf = new short[wavBufSize];
            double[] factors = new double[numChannels * 2 + sizeof(double)];
            double[] factors2 = new double[numChannels * 2 + sizeof(double)];

            //Clone array
            using (MemoryStream VagFile = new MemoryStream())
            {
                using (BinaryWriter vagWriter = new BinaryWriter(VagFile))
                {
                    List<VAGStruct> ChunksList = new List<VAGStruct>();

                    for (int pos = 0; pos < numSamples; pos += VAG_SAMPLE_NIBBL)
                    {
                        for (int buff = 0; buff < wavBufSize; buff++)
                        {
                            int index = pos + buff;
                            if (index < pcmData.Length)
                            {
                                wavBuf[buff] = pcmData[index];
                            }
                            else
                            {
                                wavBuf[buff] = 0;
                            }
                        }

                        for (int ch = 0; ch < numChannels; ch++)
                        {
                            //Put the data into the struct
                            VAGStruct VAGstruct = new VAGStruct
                            {
                                s = new byte[VAG_SAMPLE_BYTES]
                            };

                            // find_predict
                            double min = 1e10;
                            double[,] predictBuf = new double[5, 28];
                            int predict = 0, shift = 0; // prevent gcc warnings
                            double[] s1 = new double[5];
                            double[] s2 = new double[5];

                            for (int j = 0; j < 5; j++)
                            {
                                double max = 0;

                                s1[j] = factors[ch * 2];
                                s2[j] = factors[ch * 2 + 1];
                                for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                                {
                                    double sample = wavBuf[k * numChannels + ch];

                                    if (sample > 30719.0)
                                    {
                                        sample = 30719.0;
                                    }

                                    if (sample < -30720.0)
                                    {
                                        sample = -30720.0;
                                    }

                                    predictBuf[j, k] = sample - s1[j] * (VAGLut[j, 0] / 64) - s2[j] * (VAGLut[j, 1] / 64);
                                    if (Math.Abs(predictBuf[j, k]) > max)
                                    {
                                        max = Math.Abs(predictBuf[j, k]);
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
                            VAGstruct.predict_shift = (byte)(((predict << 4) & 0xF0) | (shift & 0xF));
                            VAGstruct.flag = (byte)(numSamples - pos >= 28 ? VAGFlag.VAGF_NOTHING : VAGFlag.VAGF_END_MARKER_AND_DEC);

                            sbyte[] outBuf = new sbyte[28];

                            for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                            {
                                double s_double_trans = predictBuf[predict, k] - factors2[ch * 2] * (VAGLut[predict, 0] / 64) - factors2[ch * 2 + 1] * (VAGLut[predict, 1] / 64);
                                int sample = (int)(((((int)Math.Round(s_double_trans)) << shift) + 0x800) & 0xFFFFF000);
                                if (sample > 32767)
                                {
                                    sample = 32767;
                                }

                                if (sample < -32768)
                                {
                                    sample = -32768;
                                }

                                outBuf[k] = (sbyte)(sample >> 12);
                                factors2[ch * 2 + 1] = factors2[ch * 2];
                                factors2[ch * 2] = (sample >> shift) - s_double_trans;
                            }

                            for (int k = 0; k < VAG_SAMPLE_BYTES; k++)
                            {
                                VAGstruct.s[k] = Convert.ToByte(((outBuf[k * 2 + 1] << 4) & 0xF0) | (outBuf[k * 2] & 0xF));
                            }
                            ChunksList.Add(VAGstruct);
                        }
                    }

                    //Write chunks
                    foreach (VAGStruct chunk in ChunksList)
                    {
                        vagWriter.Write(chunk.predict_shift);
                        vagWriter.Write(chunk.flag);
                        vagWriter.Write(chunk.s);
                    }

                    //Write terminating chunk
                    vagWriter.Write((byte)0);
                    vagWriter.Write((byte)VAGFlag.VAGF_END_MARKER_AND_SKIP);
                    vagWriter.Write(new byte[VAG_SAMPLE_BYTES]);

                    vagWriter.Close();
                }
                vagDat = VagFile.ToArray();
                VagFile.Close();
            }
            return vagDat;
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


