//-------------------------------------------------------------------------------------------------------------------------------
//  _____   _____ ___   __      __     _____
// |  __ \ / ____|__ \  \ \    / /\   / ____|
// | |__) | (___    ) |  \ \  / /  \ | |  __
// |  ___/ \___ \  / /    \ \/ / /\ \| | |_ |
// | |     ____) |/ /_     \  / ____ \ |__| |
// |_|    |_____/|____|     \/_/    \_\_____|
//
//-------------------------------------------------------------------------------------------------------------------------------
// VAG Format Structures
//-------------------------------------------------------------------------------------------------------------------------------
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
            sampleRate = 0;
            channels = 1;
            vagData = new byte[0];

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    if (reader.BaseStream.Length < 48)
                    {
                        throw new InvalidDataException("VAG file is smaller than its 48-byte header.");
                    }

                    if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "VAGp")
                    {
                        throw new InvalidDataException("Invalid VAG magic; expected VAGp.");
                    }

                    int version = ReadInt32BigEndian(reader);
                    if (version != 32)
                    {
                        throw new InvalidDataException("Unsupported VAG version " + version + "; expected 32.");
                    }

                    reader.BaseStream.Position = 12;
                    int declaredDataSize = ReadInt32BigEndian(reader);
                    long actualDataSize = reader.BaseStream.Length - 48;
                    if (declaredDataSize < 0 || declaredDataSize != actualDataSize)
                    {
                        throw new InvalidDataException("VAG data size mismatch: header declares " + declaredDataSize + " bytes but file contains " + actualDataSize + ".");
                    }

                    sampleRate = ReadInt32BigEndian(reader);
                    if (sampleRate <= 0 || sampleRate > 384000)
                    {
                        throw new InvalidDataException("Invalid VAG sample rate: " + sampleRate + ".");
                    }

                    reader.BaseStream.Position = 30;
                    byte channelByte = reader.ReadByte();
                    channels = channelByte == 0 || channelByte == 1 ? 1 : channelByte;
                    if (channels > 2)
                    {
                        throw new InvalidDataException("Only mono and stereo VAG files are supported; header declares " + channels + " channels.");
                    }

                    reader.BaseStream.Position = 48;
                    byte[] storedData = reader.ReadBytes(declaredDataSize);
                    int dataOffset = HasSonyInitializationBlock(storedData) ? 16 : 0;
                    int audioSize = storedData.Length - dataOffset;
                    if (audioSize <= 0 || audioSize % VAG_BLOCK_BYTES != 0)
                    {
                        throw new InvalidDataException("VAG audio data is empty or not aligned to 16-byte ADPCM blocks.");
                    }

                    vagData = new byte[audioSize];
                    Buffer.BlockCopy(storedData, dataOffset, vagData, 0, audioSize);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                sampleRate = 0;
                channels = 1;
                vagData = new byte[0];
                return false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static int ReadInt32BigEndian(BinaryReader reader)
        {
            byte[] value = reader.ReadBytes(4);
            if (value.Length != 4) throw new EndOfStreamException();
            return (value[0] << 24) | (value[1] << 16) | (value[2] << 8) | value[3];
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        private static bool HasSonyInitializationBlock(byte[] data)
        {
            if (data == null || data.Length < VAG_BLOCK_BYTES) return false;
            for (int i = 0; i < VAG_BLOCK_BYTES; i++)
            {
                if (data[i] != 0) return false;
            }
            return true;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
