using System;
using System.IO;

namespace Fox
{
    public struct Float3
    {
        public float X;
        public float Y;
        public float Z;

        public void Read(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            reader.BaseStream.Position += sizeof(float);
        }
    }

    public struct Float4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public void Reader(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadSingle();
        }
    }

    public struct Half4
    {
        public Fox.Half X;
        public Fox.Half Y;
        public Fox.Half Z;
        public Fox.Half W;

        public Half4(BinaryReader reader)
        {
            X = Fox.Half.ToHalf(reader.ReadUInt16());
            Y = Fox.Half.ToHalf(reader.ReadUInt16());
            Z = Fox.Half.ToHalf(reader.ReadUInt16());
            W = Fox.Half.ToHalf(reader.ReadUInt16());
        }
    }
}
