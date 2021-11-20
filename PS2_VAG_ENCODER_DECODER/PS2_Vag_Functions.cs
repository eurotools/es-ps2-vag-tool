using System;
using System.IO;
using System.Text;

namespace PS2_VAG_ENCODER_DECODER
{
    internal static class VAGHandler
    {
        //*===============================================================================================
        //* Definitions and Classes
        //*===============================================================================================
        private static readonly double[,] VAGLutDecoder = new double[,]
        {
            {0.0, 0.0},
            {60.0 / 64.0, 0.0},
            {115.0 / 64.0, -52.0 / 64.0},
            {98.0 / 64.0, -55.0 / 64.0},
            {122.0 / 64.0, -60.0 / 64.0}
        };

        private static readonly double[,] VAGLutEncoder = new double[,]
{
            { 0.0, 0.0 },
            {  -60.0 / 64.0, 0.0 },
            { -115.0 / 64.0, 52.0 / 64.0 },
            {  -98.0 / 64.0, 55.0 / 64.0 },
            { -122.0 / 64.0, 60.0 / 64.0 }
        };

        private struct VAGChunk
        {
            public sbyte shift;
            public sbyte predict; /* swy: reversed nibbles due to little-endian */
            public byte flags;
            public byte[] sample;
        };

        private struct AdpcmHeader
        {
            public byte[] Magic;
            public byte Version;
            public byte Channels;
            public byte Loop;
            public byte Reserved;
            public int Pitch;
            public int Samples;
        }

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
        private static int VAG_SAMPLE_BYTES = 14;
        private static int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;

        //*===============================================================================================
        //* Encoding / Decoding Functions
        //*===============================================================================================
        public static byte[] VAGEncoder(short[] pcmData, int sampleRate, uint loopOffset, bool loopFlag, bool headerless = true)
        {
            byte[] vagDat;
            bool loopOffsetSetted = false;

            double _hist_1 = 0.0, _hist_2 = 0.0;
            double hist_1 = 0.0, hist_2 = 0.0;
            double[] d_samples = new double[28];

            //double[] factors2 = new double[2 + sizeof(double)];
            int fullChunks = pcmData.Length / VAG_SAMPLE_NIBBL;

            //Clone array
            using (MemoryStream VagFile = new MemoryStream())
            {
                using (BinaryWriter vagWriter = new BinaryWriter(VagFile))
                {
                    //Write header
                    if (headerless)
                    {
                        vagWriter.Write(new byte[16]);
                    }
                    else
                    {
                        AdpcmHeader fileHeader = new AdpcmHeader
                        {
                            Magic = Encoding.ASCII.GetBytes("APCM"),
                            Version = 1,
                            Channels = 1,
                            Loop = Convert.ToByte(loopFlag),
                            Reserved = 0,
                            Pitch = ((sampleRate * 4096) / 48000),
                            Samples = pcmData.Length
                        };
                        vagWriter.Write(fileHeader.Magic);
                        vagWriter.Write(fileHeader.Version);
                        vagWriter.Write(fileHeader.Channels);
                        vagWriter.Write(fileHeader.Loop);
                        vagWriter.Write(fileHeader.Reserved);
                        vagWriter.Write(fileHeader.Pitch);
                        vagWriter.Write(fileHeader.Samples);
                    }

                    //Initialize struct where we will store the data
                    VAGChunk VAGstruct = new VAGChunk();

                    //Start packing
                    for (int pos = 0, iteration = 0; pos < pcmData.Length; pos += VAG_SAMPLE_NIBBL, iteration++)
                    {
                        // [STEP 1] --- Get chunk
                        short[] wavBuf = new short[VAG_SAMPLE_NIBBL];
                        if (iteration < fullChunks)
                        {
                            Buffer.BlockCopy(pcmData, sizeof(short) * pos, wavBuf, 0, sizeof(short) * VAG_SAMPLE_NIBBL);
                        }
                        else
                        {
                            int remainingData = pcmData.Length - pos;
                            Buffer.BlockCopy(pcmData, sizeof(short) * pos, wavBuf, 0, sizeof(short) * remainingData);
                        }

                        //Initialize samples array
                        VAGstruct.sample = new byte[VAG_SAMPLE_BYTES];

                        // [STEP 2] --- Find predict
                        int predict = 0, shift = 0;
                        double min = 1e10;
                        double s_1 = 0.0, s_2 = 0.0;
                        double[,] predictBuf = new double[28, 5];

                        for (int j = 0; j < 5; j++)
                        {
                            double max = 0.0;

                            s_1 = _hist_1;
                            s_2 = _hist_2;

                            for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                            {
                                double sample = wavBuf[k];
                                if (sample > 30719.0)
                                {
                                    sample = 30719.0;
                                }
                                if (sample < -30720.0)
                                {
                                    sample = -30720.0;
                                }

                                double ds = sample + s_1 * VAGLutEncoder[j, 0] + s_2 * VAGLutEncoder[j, 1];
                                predictBuf[k, j] = ds;
                                if (Math.Abs(ds) > max)
                                {
                                    max = Math.Abs(ds);
                                }

                                s_2 = s_1;
                                s_1 = sample;
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

                        // store s[t-2] and s[t-1] in a static variable
                        // these than used in the next function call
                        _hist_1 = s_1;
                        _hist_2 = s_2;

                        for (int i = 0; i < 28; i++)
                        {
                            d_samples[i] = predictBuf[i, predict];
                        }

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

                        //So shift==12 if none found...
                        VAGstruct.predict = (sbyte)predict;
                        VAGstruct.shift = (sbyte)shift;

                        if (pcmData.Length - pos >= VAG_SAMPLE_NIBBL)
                        {
                            if (loopFlag)
                            {
                                int currentLine = (iteration * 16) + 16;
                                if (currentLine >= loopOffset && !loopOffsetSetted)
                                {
                                    VAGstruct.flags = (byte)VAGFlag.VAGF_LOOP_START;
                                    loopOffsetSetted = true;
                                }
                                else
                                {
                                    VAGstruct.flags = (byte)VAGFlag.VAGF_LOOP_REGION;
                                }
                            }
                            else
                            {
                                VAGstruct.flags = (byte)VAGFlag.VAGF_NOTHING;
                            }
                        }
                        else
                        {
                            if (loopFlag)
                            {
                                VAGstruct.flags = (byte)VAGFlag.VAGF_LOOP_END;
                            }
                            else
                            {
                                VAGstruct.flags = (byte)VAGFlag.VAGF_LOOP_LAST_BLOCK;
                            }
                        }

                        // [STEP 4] --- Pack
                        short[] outBuf = new short[VAG_SAMPLE_NIBBL];
                        for (int k = 0; k < VAG_SAMPLE_NIBBL; k++)
                        {
                            double s_double_trans = d_samples[k] + hist_1 * VAGLutEncoder[predict, 0] + hist_2 * VAGLutEncoder[predict, 1];
                            double s_double = s_double_trans * (1 << shift);

                            int sample = (int)(((int)s_double + 0x800) & 0xFFFFF000);

                            if (sample > short.MaxValue)
                            {
                                sample = short.MaxValue;
                            }
                            if (sample < short.MinValue)
                            {
                                sample = short.MinValue;
                            }

                            outBuf[k] = (short)sample;

                            sample = sample >> shift;
                            hist_2 = hist_1;
                            hist_1 = sample - s_double_trans;
                        }

                        for (int k = 0; k < VAG_SAMPLE_BYTES; k++)
                        {
                            VAGstruct.sample[k] = (byte)(((outBuf[(k * 2) + 1] >> 8) & 0xf0) | ((outBuf[k * 2] >> 12) & 0xf));
                        }

                        // [STEP 5] --- Write
                        vagWriter.Write((byte)(((VAGstruct.predict << 4) & 0xF0) | (VAGstruct.shift & 0x0F)));
                        vagWriter.Write(VAGstruct.flags);
                        vagWriter.Write(VAGstruct.sample);
                    }

                    // put terminating chunk
                    if (!loopFlag)
                    {
                        vagWriter.Write((byte)(((VAGstruct.predict << 4) & 0xF0) | (VAGstruct.shift & 0x0F)));
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

                            double sample = (s >> vc.shift) + hist_1 * VAGLutDecoder[vc.predict, 0] + hist_2 * VAGLutDecoder[vc.predict, 1];
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

        //*===============================================================================================
        //* Other Functions
        //*===============================================================================================
        public static byte[][] SplitChannels(byte[] inout, int channels)
        {
            byte[][] tempbuf = new byte[2][];
            tempbuf[0] = new byte[inout.Length / 2];
            tempbuf[1] = new byte[inout.Length / 2];
            bool leftChannel = true;
            int IndexLC = 0, IndexRC = 0;

            using (MemoryStream vagStream = new MemoryStream(inout))
            {
                using (BinaryReader BReader = new BinaryReader(vagStream))
                {
                    //Read Stereo interleaving
                    while (BReader.BaseStream.Position < BReader.BaseStream.Length)
                    {
                        if (leftChannel)
                        {
                            Buffer.BlockCopy(BReader.ReadBytes(128), 0, tempbuf[0], IndexLC, 128);
                            IndexLC += 128;
                        }
                        else
                        {
                            Buffer.BlockCopy(BReader.ReadBytes(128), 0, tempbuf[1], IndexRC, 128);
                            IndexRC += 128;
                        }
                        leftChannel = !leftChannel;
                    }
                }
            }
            return tempbuf;
        }

        public static byte[] CombineChannelsVAG(byte[] leftChannel, byte[] rightChannel)
        {
            byte[] interleavedData;

            using(MemoryStream vagStream = new MemoryStream())
            {
                using(BinaryWriter BWriter = new BinaryWriter(vagStream))
                {
                    //Interleave channels
                    int IndexLC = 0, IndexRC = 0, i = 0;
                    while (IndexLC < leftChannel.Length || IndexRC < rightChannel.Length)
                    {
                        byte[] chunkToWrite = new byte[128];
                        if ((i % 2) == 0)
                        {
                            if (leftChannel.Length - IndexLC > 128)
                            {
                                Buffer.BlockCopy(leftChannel, IndexLC, chunkToWrite, 0, 128);
                            }
                            else
                            {
                                int remainingData = leftChannel.Length - IndexLC;
                                Buffer.BlockCopy(leftChannel, IndexLC, chunkToWrite, 0, remainingData);
                            }
                            IndexLC += 128;
                        }
                        else
                        {
                            if (rightChannel.Length - IndexRC > 128)
                            {
                                Buffer.BlockCopy(rightChannel, IndexRC, chunkToWrite, 0, 128);
                            }
                            else
                            {
                                int remainingData = rightChannel.Length - IndexRC;
                                Buffer.BlockCopy(rightChannel, IndexRC, chunkToWrite, 0, remainingData);
                            }
                            IndexRC += 128;
                        }
                        i++;
                        BWriter.Write(chunkToWrite);
                    }
                }
                interleavedData = vagStream.ToArray();
            }
            return interleavedData;
        }
    }
}