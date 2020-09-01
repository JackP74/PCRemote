using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PcRemote.Classes
{
    class Pack
    {
        public Dictionary<Type, byte> Table;

        public Pack()
        {
            Table = new Dictionary<Type, byte>
            {
                { typeof(bool), 0 },
                { typeof(byte), 1 },
                { typeof(byte[]), 2 },
                { typeof(char), 3 },
                { typeof(char[]), 4 },
                { typeof(decimal), 5 },
                { typeof(double), 6 },
                { typeof(int), 7 },
                { typeof(long), 8 },
                { typeof(sbyte), 9 },
                { typeof(short), 10 },
                { typeof(float), 11 },
                { typeof(string), 12 },
                { typeof(uint), 13 },
                { typeof(ulong), 14 },
                { typeof(ushort), 15 },
                { typeof(DateTime), 16 }
            };
        }

        public byte[] Serialize(params object[] data)
        {
            MemoryStream Stream = new MemoryStream();
            BinaryWriter Writer = new BinaryWriter(Stream, Encoding.UTF8);
            byte Current = 0;
            Writer.Write(Convert.ToByte(data.Length));
            for (int I = 0; I <= data.Length - 1; I++)
            {
                Current = Table[data[I].GetType()];
                Writer.Write(Current);
                switch (Current)
                {
                    case 0:
                        Writer.Write((bool)data[I]);
                        break;
                    case 1:
                        Writer.Write((byte)data[I]);
                        break;
                    case 2:
                        Writer.Write(((byte[])data[I]).Length);
                        Writer.Write((byte[])data[I]);
                        break;
                    case 3:
                        Writer.Write((char)data[I]);
                        break;
                    case 4:
                        Writer.Write(((char[])data[I]).ToString());
                        break;
                    case 5:
                        Writer.Write((decimal)data[I]);
                        break;
                    case 6:
                        Writer.Write((double)data[I]);
                        break;
                    case 7:
                        Writer.Write((int)data[I]);
                        break;
                    case 8:
                        Writer.Write((long)data[I]);
                        break;
                    case 9:
                        Writer.Write((sbyte)data[I]);
                        break;
                    case 10:
                        Writer.Write((short)data[I]);
                        break;
                    case 11:
                        Writer.Write((float)data[I]);
                        break;
                    case 12:
                        Writer.Write((string)data[I]);
                        break;
                    case 13:
                        Writer.Write((uint)data[I]);
                        break;
                    case 14:
                        Writer.Write((ulong)data[I]);
                        break;
                    case 15:
                        Writer.Write((ushort)data[I]);
                        break;
                    case 16:
                        Writer.Write(((System.DateTime)data[I]).ToBinary());
                        break;
                }
            }
            Writer.Close();
            return Stream.ToArray();
        }

        public object[] Deserialize(byte[] data)
        {
            try
            {
                MemoryStream Stream = new MemoryStream(data);
                BinaryReader Reader = new BinaryReader(Stream, Encoding.UTF8);
                List<object> Items = new List<object>();
                byte Current = 0;
                byte Count = Reader.ReadByte();
                for (int I = 0; I <= Count - 1; I++)
                {
                    Current = Reader.ReadByte();
                    switch (Current)
                    {
                        case 0:
                            Items.Add(Reader.ReadBoolean());
                            break;
                        case 1:
                            Items.Add(Reader.ReadByte());
                            break;
                        case 2:
                            Items.Add(Reader.ReadBytes(Reader.ReadInt32()));
                            break;
                        case 3:
                            Items.Add(Reader.ReadChar());
                            break;
                        case 4:
                            Items.Add(Reader.ReadString().ToCharArray());
                            break;
                        case 5:
                            Items.Add(Reader.ReadDecimal());
                            break;
                        case 6:
                            Items.Add(Reader.ReadDouble());
                            break;
                        case 7:
                            Items.Add(Reader.ReadInt32());
                            break;
                        case 8:
                            Items.Add(Reader.ReadInt64());
                            break;
                        case 9:
                            Items.Add(Reader.ReadSByte());
                            break;
                        case 10:
                            Items.Add(Reader.ReadInt16());
                            break;
                        case 11:
                            Items.Add(Reader.ReadSingle());
                            break;
                        case 12:
                            Items.Add(Reader.ReadString());
                            break;
                        case 13:
                            Items.Add(Reader.ReadUInt32());
                            break;
                        case 14:
                            Items.Add(Reader.ReadUInt64());
                            break;
                        case 15:
                            Items.Add(Reader.ReadUInt16());
                            break;
                        case 16:
                            Items.Add(DateTime.FromBinary(Reader.ReadInt64()));
                            break;
                    }
                }
                Reader.Close();
                return Items.ToArray();
            }
            catch 
            {
                return null;
            }
        }
    }
}