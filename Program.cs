using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Fox
{
    class Program
    {
        public class Chunk00
        {
            public uint StartPosition;
            public uint NextSectionOffset;
            public uint Size { get { return StartPosition - NextSectionOffset; } }

            public uint Hash;
            public uint U2;
            public uint Flag;
            public uint Flag2;

            public void Read(BinaryReader reader)
            {
                StartPosition = (uint)reader.BaseStream.Position;

                Hash = reader.ReadUInt32();
                U2 = reader.ReadUInt32();
                Flag = reader.ReadUInt32();

                NextSectionOffset = reader.ReadUInt32();

                Flag2 = reader.ReadUInt32();
            }
        }

        public class Chunk16
        {
            public uint StartPosition;
            public uint NextSectionOffset;
            public uint Size { get { return StartPosition - NextSectionOffset; } }

            public uint[] Vals; // 4
            public uint DataChunkOffset;
            public uint EndOfChunk17Offset;
            public uint[] UnknownInts;

            // Virtual children
            public List<DataChunk> DataChunks;

            public void Read(BinaryReader reader)
            {
                // Save reader position
                StartPosition = (uint)reader.BaseStream.Position;

                // Unknown/throwaway variables
                Vals = new uint[4];
                for (uint i = 0; i < 4; i++)
                    Vals[i] = reader.ReadUInt32();

                DataChunkOffset = StartPosition + reader.ReadUInt32();
                EndOfChunk17Offset = reader.ReadUInt32() + (uint)reader.BaseStream.Position;

                // The semi-existing "Chunk 17" has a list of uints but apparent count and even this approach leaves a big gap between it and the DataChunks
                uint unknownIntSize = (EndOfChunk17Offset - (uint)reader.BaseStream.Position) / sizeof(uint);
                UnknownInts = new uint[unknownIntSize];
                for (uint i = 0; i < unknownIntSize; i++)
                    UnknownInts[i] = reader.ReadUInt32();

                reader.BaseStream.Position = DataChunkOffset;

                // TODO: DataChunks
            }
        };

        public struct DataChunk
        {
            public uint StartPosition;
            public uint NextEntryOffset;
            public uint Size { get { return StartPosition - NextEntryOffset; } }

            public void Read(BinaryReader reader)
            {

            }
        }

        public class Entry
        {
            public uint StartPosition;
            public uint NextEntryOffset;
            public uint Size { get { return StartPosition - NextEntryOffset; } }

            // Types
            public uint Hash;
            public uint U2;
            public uint Flag;
            public uint U4; // 0
            public uint U5; // 0
            public uint U6; // 0
            public short U8A;
            public short U8B;

            // Virtual children
            public Chunk00 Chunk0;
            //private Chunk10 Chunk10; // mutually exclusive with 16/17
            public Chunk16 Chunk16; // mutually exclusive with 16/17
            public List<DataChunk> Chunks;

            public void Read(BinaryReader reader)
            {
                // Save reader position
                StartPosition = (uint)reader.BaseStream.Position;

                // Unknown/throwaway variables
                Hash = reader.ReadUInt32();
                U2 = reader.ReadUInt32();
                Flag = reader.ReadUInt32();
                U4 = reader.ReadUInt32(); // 0
                U5 = reader.ReadUInt32(); // 0
                U6 = reader.ReadUInt32(); // 0

                uint nextSectionOffset = reader.ReadUInt32();

                U8A = reader.ReadInt16();
                U8B = reader.ReadInt16();

                NextEntryOffset = StartPosition + reader.ReadUInt32();

                // Offset to Chunk0
                reader.BaseStream.Position = StartPosition + nextSectionOffset;

                // Read Chunk0
                Chunk0 = new Chunk00();
                Chunk0.Read(reader);

                // Exit early - HACK
                if (Chunk0.NextSectionOffset == 0)
                    return;

                // Offset for either chunk10 or chunk16
                reader.BaseStream.Position = StartPosition + nextSectionOffset + Chunk0.NextSectionOffset;

                if (Chunk0.Flag == 0)
                {

                }
                else if (Chunk0.Flag == 6)
                {
                    Chunk16 = new Chunk16();
                    Chunk16.Read(reader);
                }

                //while (reader.BaseStream.Position < StartPosition + Size)
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            using (var reader = new BinaryReader(new FileStream(args[0], FileMode.Open)))
            {
                Debug.Assert(reader.ReadUInt32() == 201406020);

                uint firstEntryOffset = reader.ReadUInt32();
                uint fileLength = reader.ReadUInt32();

                Debug.Assert(reader.ReadUInt32() == 7050094);

                reader.BaseStream.Position += 2 * sizeof(uint);

                uint hash1 = reader.ReadUInt32();
                uint hash2 = reader.ReadUInt32();

                // Entries
                List<Entry> entries = new List<Entry>();
                uint nextEntryOffset = firstEntryOffset;
                bool nextEntryExists = unchecked((int)nextEntryOffset) != 0;
                while (nextEntryExists)
                {
                    reader.BaseStream.Position = nextEntryOffset;

                    Entry entry = new Entry();
                    entry.Read(reader);
                    entries.Add(entry);

                    nextEntryExists = entry.NextEntryOffset - nextEntryOffset != 0;
                    nextEntryOffset = entry.NextEntryOffset;
                };
            }
        }     
    }
}
