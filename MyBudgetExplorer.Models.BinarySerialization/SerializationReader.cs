// Original code author: Tim Haynes, May 2006.  
// Use freely as you see fit.
// Copied from codeproject.com many moons ago and modified slightly.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyBudgetExplorer.Models.BinarySerialization
{
    public class SerializationReader : BinaryReader
    {
        #region Properties
        public new Stream BaseStream { get; private set; }
        #endregion

        #region Constructors
        private SerializationReader(Stream s) : base(s)
        {
            BaseStream = s;
        }
        #endregion

        #region Public Methods
        public static SerializationReader GetReader(SerializationInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var byteArray = (byte[])info.GetValue("X", typeof(byte[]));
            var ms = new MemoryStream(byteArray);
            return new SerializationReader(ms);
        }
        public byte[] ReadByteArray()
        {
            var len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            return len < 0 ? null : new byte[0];
        }
        private char[] ReadCharArray()
        {
            var len = ReadInt32();
            if (len > 0) return ReadChars(len);
            return len < 0 ? null : new char[0];
        }
        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64());
        }
        public IDictionary<T, TU> ReadDictionary<T, TU>()
        {
            var count = ReadInt32();
            if (count < 0) return null;
            IDictionary<T, TU> d = new Dictionary<T, TU>();
            for (var i = 0; i < count; i++) d[(T)ReadObject()] = (TU)ReadObject();
            return d;
        }
        public IList<T> ReadList<T>()
        {
            var count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>();
            for (var i = 0; i < count; i++) d.Add((T)ReadObject());
            return d;
        }
        public object ReadObject()
        {
            var t = (ObjectType)ReadByte();
            switch (t)
            {
                case ObjectType.boolType:
                    return ReadBoolean();
                case ObjectType.byteType:
                    return ReadByte();
                case ObjectType.uint16Type:
                    return ReadUInt16();
                case ObjectType.uint32Type:
                    return ReadUInt32();
                case ObjectType.uint64Type:
                    return ReadUInt64();
                case ObjectType.sbyteType:
                    return ReadSByte();
                case ObjectType.int16Type:
                    return ReadInt16();
                case ObjectType.int32Type:
                    return ReadInt32();
                case ObjectType.int64Type:
                    return ReadInt64();
                case ObjectType.charType:
                    return ReadChar();
                case ObjectType.stringType:
                    return base.ReadString();
                case ObjectType.singleType:
                    return ReadSingle();
                case ObjectType.doubleType:
                    return ReadDouble();
                case ObjectType.decimalType:
                    return ReadDecimal();
                case ObjectType.dateTimeType:
                    return ReadDateTime();
                case ObjectType.byteArrayType:
                    return ReadByteArray();
                case ObjectType.charArrayType:
                    return ReadCharArray();
                case ObjectType.otherType:
                    return new BinaryFormatter().Deserialize(this.BaseStream);
                default:
                    return null;
            }
        }
        public override string ReadString()
        {
            var t = (ObjectType)ReadByte();

            if (t == ObjectType.nullType)
                return null;

            return t == ObjectType.stringType ? base.ReadString() : string.Empty;
        }
        #endregion
    }
}
