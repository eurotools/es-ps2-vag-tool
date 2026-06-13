using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS2VagTool.Audio
{
    internal static class AiffLoopReader
    {
        private sealed class AiffMarker
        {
            internal short Id;
            internal uint Position;
        }

        private struct AiffChunk
        {
            internal AiffChunk(long start, string id, uint length)
            {
                Start = start;
                Id = id;
                Length = length;
            }

            internal long Start;
            internal string Id;
            internal uint Length;
        }

        internal static AudioLoopInfo ReadLoop(Stream stream, long totalSampleFrames)
        {
            BinaryReader reader = new BinaryReader(stream);
            EnsureAiffHeader(reader);

            List<AiffMarker> markers = new List<AiffMarker>();
            bool hasInstrumentChunk = false;
            short sustainPlayMode = 0;
            short sustainBeginMarkerId = 0;
            short sustainEndMarkerId = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                AiffChunk chunk = ReadChunkHeader(reader);
                long nextChunkPosition = GetNextChunkPosition(chunk);

                if (chunk.Id == "MARK")
                {
                    ReadMarkerChunk(reader, chunk, markers);
                }
                else if (chunk.Id == "INST" && chunk.Length >= 20)
                {
                    reader.ReadBytes(8);
                    sustainPlayMode = ReadInt16BigEndian(reader);
                    sustainBeginMarkerId = ReadInt16BigEndian(reader);
                    sustainEndMarkerId = ReadInt16BigEndian(reader);
                    hasInstrumentChunk = true;
                }

                reader.BaseStream.Position = Math.Min(nextChunkPosition, reader.BaseStream.Length);
                if (chunk.Id == "\0\0\0\0")
                {
                    break;
                }
            }

            if (hasInstrumentChunk)
            {
                return BuildLoopFromInstrument(markers, sustainPlayMode, sustainBeginMarkerId, sustainEndMarkerId, totalSampleFrames);
            }

            return BuildCompatibilityLoopFromMarkers(markers, totalSampleFrames);
        }

        private static AudioLoopInfo BuildLoopFromInstrument(List<AiffMarker> markers, short playMode, short beginMarkerId, short endMarkerId, long totalSampleFrames)
        {
            if (playMode == 0)
            {
                return new AudioLoopInfo(false, 0, 0, null, "AIFF INST chunk has no sustain loop.");
            }

            if (playMode != 1)
            {
                return new AudioLoopInfo(false, 0, 0, null, "AIFF sustain loop is not a supported forward loop.");
            }

            AiffMarker startMarker = FindMarker(markers, beginMarkerId);
            AiffMarker endMarker = FindMarker(markers, endMarkerId);
            if (startMarker == null || endMarker == null)
            {
                return new AudioLoopInfo(false, 0, 0, null, "AIFF INST loop references missing MARK entries.");
            }

            return LoopValidator.Validate(startMarker.Position, endMarker.Position, totalSampleFrames, "AIFF INST");
        }

        private static AudioLoopInfo BuildCompatibilityLoopFromMarkers(List<AiffMarker> markers, long totalSampleFrames)
        {
            if (markers.Count >= 2)
            {
                AudioLoopInfo markerLoop = LoopValidator.Validate(markers[0].Position, markers[1].Position, totalSampleFrames, "AIFF MARK");
                if (markerLoop.IsLooped)
                {
                    return new AudioLoopInfo(true, markerLoop.StartSample, markerLoop.EndSample, "AIFF MARK", "AIFF has MARK entries but no INST loop; using the first two markers for compatibility.");
                }

                return markerLoop;
            }

            if (markers.Count == 1)
            {
                return new AudioLoopInfo(false, 0, 0, null, "AIFF has only one MARK entry, so no loop was applied.");
            }

            return AudioLoopInfo.None;
        }

        private static void EnsureAiffHeader(BinaryReader reader)
        {
            if (ReadChunkId(reader) != "FORM")
            {
                throw new FormatException("Not an AIFF file - no FORM header.");
            }

            ReadUInt32BigEndian(reader);
            if (ReadChunkId(reader) != "AIFF")
            {
                throw new FormatException("Not an AIFF file - no AIFF header.");
            }
        }

        private static AiffChunk ReadChunkHeader(BinaryReader reader)
        {
            return new AiffChunk(reader.BaseStream.Position, ReadChunkId(reader), ReadUInt32BigEndian(reader));
        }

        private static void ReadMarkerChunk(BinaryReader reader, AiffChunk chunk, List<AiffMarker> markers)
        {
            if (chunk.Length < 2)
            {
                return;
            }

            short markerCount = ReadInt16BigEndian(reader);
            long chunkEnd = chunk.Start + 8 + chunk.Length;
            for (int i = 0; i < markerCount && reader.BaseStream.Position + 7 <= chunkEnd; i++)
            {
                short id = ReadInt16BigEndian(reader);
                uint position = ReadUInt32BigEndian(reader);
                ReadPascalString(reader, chunkEnd);

                markers.Add(new AiffMarker
                {
                    Id = id,
                    Position = position
                });
            }
        }

        private static string ReadPascalString(BinaryReader reader, long chunkEnd)
        {
            int declaredLength = reader.ReadByte();
            int readableLength = (int)Math.Min(declaredLength, Math.Max(0, chunkEnd - reader.BaseStream.Position));
            string value = Encoding.ASCII.GetString(reader.ReadBytes(readableLength));

            int bytesToSkip = declaredLength - readableLength;
            if (bytesToSkip > 0)
            {
                reader.ReadBytes(bytesToSkip);
            }

            if (((1 + declaredLength) % 2) != 0 && reader.BaseStream.Position < chunkEnd)
            {
                reader.ReadByte();
            }

            return value;
        }

        private static AiffMarker FindMarker(List<AiffMarker> markers, short id)
        {
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].Id == id)
                {
                    return markers[i];
                }
            }

            return null;
        }

        private static long GetNextChunkPosition(AiffChunk chunk)
        {
            long nextPosition = chunk.Start + 8 + chunk.Length;
            if ((chunk.Length % 2) != 0)
            {
                nextPosition++;
            }

            return nextPosition;
        }

        private static string ReadChunkId(BinaryReader reader)
        {
            return new string(reader.ReadChars(4));
        }

        private static short ReadInt16BigEndian(BinaryReader reader)
        {
            byte[] buffer = reader.ReadBytes(2);
            if (buffer.Length != 2)
            {
                throw new EndOfStreamException();
            }

            return (short)((buffer[0] << 8) | buffer[1]);
        }

        private static uint ReadUInt32BigEndian(BinaryReader reader)
        {
            byte[] buffer = reader.ReadBytes(4);
            if (buffer.Length != 4)
            {
                throw new EndOfStreamException();
            }

            return (uint)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]);
        }
    }
}
