using System;
using System.IO;
using System.Text;

namespace PS2VagTool.Vag
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    public static partial class SonyVag
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        private struct VAGChunk
        {
            public sbyte shift;
            public sbyte predict; /* swy: reversed nibbles due to little-endian */
            public byte flags;
            public byte[] sample;
        };

        //-------------------------------------------------------------------------------------------------------------------------------
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

        //-------------------------------------------------------------------------------------------------------------------------------
        private const int VAG_BLOCK_BYTES = 16;
        private const int VAG_BLOCK_SAMPLES = 28;
        private static readonly int VAG_SAMPLE_BYTES = 14;
        private static readonly int VAG_SAMPLE_NIBBL = VAG_SAMPLE_BYTES * 2;

        //-------------------------------------------------------------------------------------------------------------------------------
        public static uint GetLoopByteOffsetForSample(uint sampleOffset, int channelCount)
        {
            if (channelCount < 1)
            {
                channelCount = 1;
            }

            return (uint)(((sampleOffset * VAG_BLOCK_BYTES) / VAG_BLOCK_SAMPLES) * channelCount);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        public static uint GetLoopBlockIndexForSample(uint sampleOffset, int channelCount)
        {
            uint byteOffset = GetLoopByteOffsetForSample(sampleOffset, channelCount);
            return byteOffset / (uint)(VAG_BLOCK_BYTES * Math.Max(channelCount, 1));
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool VagFileIsValid(string inputFile, out int sampleRate, out byte[] vagData)
        {
            return VagFileIsValid(inputFile, out sampleRate, out int channels, out vagData);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        internal static bool VagFileIsValid(string inputFile, out int sampleRate, out int channels, out byte[] vagData)
        {
            bool FileIsValid = true;
            sampleRate = 0;
            channels = 1;
            vagData = new byte[0];

            //Read VAG Header
            using (BinaryReader binReader = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                string Magic = Encoding.ASCII.GetString(binReader.ReadBytes(4));
                if (Magic.Equals("VAGp"))
                {
                    int FileVersion = ProgramFunctions.FlipInt32(binReader.ReadInt32());
                    if (FileVersion == 32)
                    {
                        //Get Sample Rate
                        binReader.BaseStream.Seek(16, SeekOrigin.Begin);
                        sampleRate = ProgramFunctions.FlipInt32(binReader.ReadInt32());

                        //Get Num Of Channels
                        binReader.BaseStream.Seek(30, SeekOrigin.Begin);
                        byte channelByte = binReader.ReadByte();
                        channels = channelByte > 1 ? channelByte : 1;
                        if (channels > 2)
                        {
                            FileIsValid = false;
                            Console.WriteLine("ERROR: This decoder only supports mono and stereo files.");
                        }
                        else
                        {
                            binReader.BaseStream.Seek(48, SeekOrigin.Begin);
                            int totalSize = (int)(binReader.BaseStream.Length - 0x30);
                            vagData = binReader.ReadBytes(totalSize);
                        }
                    }
                    else
                    {
                        FileIsValid = false;
                        Console.WriteLine("ERROR: The file version is not supported, file version: " + FileVersion + " supported version: 32.");
                    }
                }
                else
                {
                    FileIsValid = false;
                    Console.WriteLine("ERROR: Invalid file type.");
                }
            }

            return FileIsValid;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
