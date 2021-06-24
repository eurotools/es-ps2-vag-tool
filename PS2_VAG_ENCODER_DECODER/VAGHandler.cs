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
            VAGF_NOTHING = 0,         /* Nothing*/
            VAGF_LOOP_LAST_BLOCK = 1, /* Last block to loop */
            VAGF_LOOP_REGION = 2,     /* Loop region*/
            VAGF_LOOP_END = 3,        /* Ending block of the loop */
            VAGF_LOOP_FIRST_BLOCK = 4,/* First block of looped data */
            VAGF_UNK = 5,             /* ?*/
            VAGF_LOOP_START = 6,      /* Starting block of the loop*/
            VAGF_PLAYBACK_END = 7     /* Playback ending position */
        };

        //Defines
        private static int VAG_MAX_LUT_INDX = VAGLut.Length - 1;
        private static int VAG_SAMPLE_BYTES = 14;
        private static int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;

        //*===============================================================================================
        //* Encoding / Decoding Functions
        //*===============================================================================================
        public static byte[] VAGEncoder(short[] pcmData, int bitDepth, uint loopOffset, bool loopFlag)
        {
            byte[] vagDat;
            const int numChannels = 1;

            // size compression is 28 16-bit samples -> 16 bytes
            int s2size = pcmData.Length * numChannels * bitDepth / 8;
            int sampleSize = (numChannels * bitDepth) / 8;

            uint numSamples = ((uint)(s2size / sampleSize));

            short[] wavBuf;
            double[] factors = new double[numChannels * 2 + sizeof(double)];
            double[] factors2 = new double[numChannels * 2 + sizeof(double)];

            //Clone array
            using (MemoryStream VagFile = new MemoryStream())
            {
                using (BinaryWriter vagWriter = new BinaryWriter(VagFile))
                {
                    //Write first line
                    vagWriter.Write(new byte[16]);

                    //Start packing
                    for (int pos = 0, iteration = 0; pos < numSamples; pos += VAG_SAMPLE_NIBBL, iteration++)
                    {
                        // [STEP 1] --- Get chunk
                        wavBuf = new short[VAG_SAMPLE_NIBBL];
                        for (int buff = 0; buff < VAG_SAMPLE_NIBBL; buff++)
                        {
                            int index = pos + buff;
                            if (index >= pcmData.Length)
                            {
                                break;
                            }
                            wavBuf[buff] = pcmData[index];
                        }

                        //Put the data into the struct
                        VAGChunk VAGstruct = new VAGChunk
                        {
                            s = new byte[VAG_SAMPLE_BYTES]
                        };

                        // [STEP 2] --- Find predict
                        int predict = 0, shift = 0;
                        double min = 1e10;
                        double[] s1 = new double[5];
                        double[] s2 = new double[5];
                        double[,] predictBuf = new double[5, 28];

                        for (int j = 0; j < 5; j++)
                        {
                            double max = 0;

                            s1[j] = factors[2];
                            s2[j] = factors[2 + 1];
                            for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                            {
                                double sample = Math.Min(30719.0, Math.Max(wavBuf[k], -30720.0));
                                predictBuf[j, k] = sample - s1[j] * ((double)VAGLut[j, 0] / 64) - s2[j] * ((double)VAGLut[j, 1] / 64);

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
                            if (min <= 7)
                            {
                                predict = 0;
                                break;
                            }
                        }
                        factors[2] = s1[predict];
                        factors[2 + 1] = s2[predict];

                        // [STEP 3] --- Find shift
                        int min2 = (int)min;
                        int shift_mask = 0x4000;
                        shift = 0;

                        while (shift < 12)
                        {
                            if (Convert.ToBoolean(shift_mask & (min2 + (shift_mask >> 3))))
                            {
                                break;
                            }
                            shift++;
                            shift_mask = shift_mask >> 1;
                        }

                        // so shift==12 if none found...
                        VAGstruct.predictNR = (sbyte)predict;
                        VAGstruct.shiftFactor = (sbyte)shift;

                        if (numSamples - pos >= VAG_SAMPLE_NIBBL)
                        {
                            if (loopFlag)
                            {
                                int currentPos = iteration * 16;
                                if (currentPos == loopOffset)
                                {
                                    VAGstruct.flag = (byte)VAGFlag.VAGF_LOOP_START;
                                }
                                else
                                {
                                    VAGstruct.flag = (byte)VAGFlag.VAGF_LOOP_REGION;
                                }
                            }
                            else
                            {
                                VAGstruct.flag = (byte)VAGFlag.VAGF_NOTHING;
                            }
                        }
                        else
                        {
                            if (loopFlag)
                            {
                                VAGstruct.flag = (byte)VAGFlag.VAGF_LOOP_END;
                            }
                            else
                            {
                                VAGstruct.flag = (byte)VAGFlag.VAGF_LOOP_LAST_BLOCK;
                            }
                        }

                        // [STEP 4] --- Pack
                        short[] outBuf = new short[VAG_SAMPLE_NIBBL];
                        for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                        {
                            double s_double_trans = predictBuf[predict, k] - factors2[2] * ((double)VAGLut[predict, 0] / 64) - factors2[2 + 1] * ((double)VAGLut[predict, 1] / 64);
                            double s_double = s_double_trans * (1 << shift);
                            int sample = (int)(((int)s_double + 0x800) & 0xFFFFF000);
                            sample = Math.Min(short.MaxValue, Math.Max(sample, short.MinValue));

                            outBuf[k] = (short)sample;
                            factors2[2 + 1] = factors2[2];
                            factors2[2] = (sample >> shift) - s_double_trans;
                        }

                        for (int k = 0; k < VAG_SAMPLE_BYTES; k++)
                        {
                            VAGstruct.s[k] = Convert.ToByte(((outBuf[(k * 2) + 1] >> 8) & 0xF0) | ((outBuf[k * 2] >> 12) & 0x0F));
                        }

                        // [STEP 5] --- Write
                        vagWriter.Write((byte)(((predict << 4) & 0xF0) | (shift & 0xF)));
                        vagWriter.Write(VAGstruct.flag);
                        vagWriter.Write(VAGstruct.s);
                    }

                    // put terminating chunk
                    if (!loopFlag)
                    {
                        vagWriter.Write((byte)0);
                        vagWriter.Write((byte)VAGFlag.VAGF_PLAYBACK_END);
                        vagWriter.Write(new byte[VAG_SAMPLE_BYTES]);
                    }

                    //Close
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

                    if (VAGstruct.flag == (int)VAGFlag.VAGF_PLAYBACK_END)
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


