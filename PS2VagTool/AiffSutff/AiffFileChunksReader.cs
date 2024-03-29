﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS2VagTool
{
    /// <summary>A read-only stream of AIFF data based on an aiff file
    /// with an associated WaveFormat
    /// originally contributed to NAudio by Giawa
    /// </summary>
    public class AiffFileChunksReader : WaveStream
    {
        #region Endian Helpers
        private static uint ConvertLong(byte[] buffer)
        {
            if (buffer.Length != 4) throw new ArgumentException("Incorrect length");
            return (uint)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]);
        }

        private static short ConvertShort(byte[] buffer)
        {
            if (buffer.Length != 2) throw new ArgumentException("Incorrect length or short.");
            return (short)((buffer[0] << 8) | buffer[1]);
        }
        #endregion

        #region AiffChunk
        /// <summary>
        /// AIFF Chunk
        /// </summary>
        public struct AiffChunk
        {
            /// <summary>
            /// Chunk Name
            /// </summary>
            public string chunkName;
            /// <summary>
            /// Chunk Length
            /// </summary>
            public uint chunkLength;
            /// <summary>
            /// Chunk start
            /// </summary>
            public uint chunkStart;

            /// <summary>
            /// Creates a new AIFF Chunk
            /// </summary>
            public AiffChunk(uint start, string name, uint length)
            {
                chunkStart = start;
                chunkName = name;
                chunkLength = length;
            }
        }

        private static AiffChunk ReadChunkHeader(BinaryReader br)
        {
            AiffChunk chunk = new AiffChunk((uint)br.BaseStream.Position, ReadChunkName(br), ConvertLong(br.ReadBytes(4)));
            return chunk;
        }

        private static string ReadChunkName(BinaryReader br)
        {
            return new string(br.ReadChars(4));
        }
        #endregion

        private readonly WaveFormat waveFormat;
        private Stream waveStream;
        private readonly bool ownInput;
        private readonly long dataPosition;
        private readonly int dataChunkLength;
        private readonly List<MarkerChunkData> markers = new List<MarkerChunkData>();

        /// <summary>Supports opening a AIF file</summary>
        /// <remarks>The AIF is of similar nastiness to the WAV format.
        /// This supports basic reading of uncompressed PCM AIF files,
        /// with 16, 24 and 32 bit PCM data.
        /// </remarks>
        public AiffFileChunksReader(String aiffFile) :
            this(File.OpenRead(aiffFile))
        {
            ownInput = true;
        }

        /// <summary>
        /// Creates an Aiff File Reader based on an input stream
        /// </summary>
        /// <param name="inputStream">The input stream containing a AIF file including header</param>
        public AiffFileChunksReader(Stream inputStream)
        {
            this.waveStream = inputStream;
            ReadAiffHeader(waveStream, markers);
            Position = 0;
        }

        /// <summary>
        /// Ensures valid AIFF header and then finds data offset.
        /// </summary>
        /// <param name="stream">The stream, positioned at the start of audio data</param>
        /// <param name="format">The format found</param>
        /// <param name="dataChunkPosition">The position of the data chunk</param>
        /// <param name="dataChunkLength">The length of the data chunk</param>
        /// <param name="chunks">Additional chunks found</param>
        public static void ReadAiffHeader(Stream stream, List<MarkerChunkData> markers)
        {
            BinaryReader br = new BinaryReader(stream);

            if (ReadChunkName(br) != "FORM")
            {
                throw new FormatException("Not an AIFF file - no FORM header.");
            }
            ConvertLong(br.ReadBytes(4));
            if (ReadChunkName(br) != "AIFF")
            {
                throw new FormatException("Not an AIFF file - no AIFF header.");
            }

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                AiffChunk nextChunk = ReadChunkHeader(br);
                if (nextChunk.chunkName == "MARK")
                {
                    short numOfMarkers = ConvertShort(br.ReadBytes(2));
                    for (int i = 0; i < numOfMarkers; i++)
                    {
                        MarkerChunkData markerData = new MarkerChunkData
                        {
                            id = ConvertShort(br.ReadBytes(2)),
                            position = ConvertLong(br.ReadBytes(4)),
                            markerName = string.Join("", br.ReadByte(), Encoding.ASCII.GetString(br.ReadBytes(8)), br.ReadByte())
                        };
                        markers.Add(markerData);
                    }
                }
                else
                {
                    br.ReadBytes((int)nextChunk.chunkLength);
                }

                if (nextChunk.chunkName == "\0\0\0\0") break;
            }
        }

        /// <summary>
        /// Cleans up the resources associated with this AiffFileReader
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                if (waveStream != null)
                {
                    // only dispose our source if we created it
                    if (ownInput)
                    {
                        waveStream.Close();
                    }
                    waveStream = null;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "AiffFileReader was not disposed");
            }
            // Release unmanaged resources.
            // Set large fields to null.
            // Call Dispose on your base class.
            base.Dispose(disposing);
        }

        /// <summary>
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get
            {
                return waveFormat;
            }
        }

        /// <summary>
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public override long Length
        {
            get
            {
                return dataChunkLength;
            }
        }

        /// <summary>
        /// Number of Samples (if possible to calculate)
        /// </summary>
        public long SampleCount
        {
            get
            {
                if (waveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.Pcm ||
                    waveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.Extensible ||
                    waveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.IeeeFloat)
                {
                    return dataChunkLength / BlockAlign;
                }
                else
                {
                    throw new FormatException("Sample count is calculated only for the standard encodings");
                }
            }
        }

        /// <summary>
        /// Position in the AIFF file
        /// <see cref="Stream.Position"/>
        /// </summary>
        public override long Position
        {
            get
            {
                return waveStream.Position - dataPosition;
            }
            set
            {
                lock (this)
                {
                    value = Math.Min(value, Length);
                    // make sure we don't get out of sync
                    value -= (value % waveFormat.BlockAlign);
                    waveStream.Position = value + dataPosition;
                }
            }
        }


        /// <summary>
        /// Reads bytes from the AIFF File
        /// <see cref="Stream.Read"/>
        /// </summary>
        public override int Read(byte[] array, int offset, int count)
        {
            if (count % waveFormat.BlockAlign != 0)
            {
                throw new ApplicationException(String.Format("Must read complete blocks: requested {0}, block align is {1}", count, this.WaveFormat.BlockAlign));
            }
            // sometimes there is more junk at the end of the file past the data chunk
            if (Position + count > dataChunkLength)
            {
                count = dataChunkLength - (int)Position;
            }

            // Need to fix the endianness since intel expect little endian, and apple is big endian.
            byte[] buffer = new byte[count];
            int length = waveStream.Read(buffer, offset, count);

            int bytesPerSample = WaveFormat.BitsPerSample / 8;
            for (int i = 0; i < length; i += bytesPerSample)
            {
                if (WaveFormat.BitsPerSample == 16)
                {
                    array[i + 0] = buffer[i + 1];
                    array[i + 1] = buffer[i];
                }
                else if (WaveFormat.BitsPerSample == 24)
                {
                    array[i + 0] = buffer[i + 2];
                    array[i + 1] = buffer[i + 1];
                    array[i + 2] = buffer[i + 0];
                }
                else if (WaveFormat.BitsPerSample == 32)
                {
                    array[i + 0] = buffer[i + 3];
                    array[i + 1] = buffer[i + 2];
                    array[i + 2] = buffer[i + 1];
                    array[i + 3] = buffer[i + 0];
                }
                else throw new Exception("Unsupported PCM format.");
            }

            return length;
        }
    }
}
