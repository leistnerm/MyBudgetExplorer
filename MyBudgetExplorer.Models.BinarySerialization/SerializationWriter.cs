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
    public class SerializationWriter : BinaryWriter
    {
        #region Constructors
        private SerializationWriter(Stream s) : base(s) { }
        #endregion

        #region Public Methods
        public void AddToInfo(SerializationInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var b = ((MemoryStream)BaseStream).ToArray();
            info.AddValue("X", b, typeof(byte[]));
        }
        public static SerializationWriter GetWriter()
        {
            var ms = new MemoryStream(1024);
            return new SerializationWriter(ms);
        }
        public override void Write(char[] chars)
        {
            var len = chars.Length;
            Write(len);
            if (len > 0) base.Write(chars);
        }
        public void Write(DateTime dt)
        {
            Write(dt.Ticks);
        }
        public void Write<T>(ICollection<T> c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                foreach (var item in c) WriteObject(item);
            }
        }
        public void Write<T, TU>(IDictionary<T, TU> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (var kvp in d)
                {
                    WriteObject(kvp.Key);
                    WriteObject(kvp.Value);
                }
            }
        }
        public void WriteByteArray(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                var len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }
        public void WriteObject(object value)
        {
            if (value == null)
            {
                Write((byte)ObjectType.nullType);
            }
            else
            {
                switch (value.GetType().Name)
                {
                    case "Boolean":
                        Write((byte)ObjectType.boolType);
                        Write((bool)value);
                        break;

                    case "Byte":
                        Write((byte)ObjectType.byteType);
                        Write((byte)value);
                        break;

                    case "UInt16":
                        Write((byte)ObjectType.uint16Type);
                        Write((ushort)value);
                        break;

                    case "UInt32":
                        Write((byte)ObjectType.uint32Type);
                        Write((uint)value);
                        break;

                    case "UInt64":
                        Write((byte)ObjectType.uint64Type);
                        Write((ulong)value);
                        break;

                    case "SByte":
                        Write((byte)ObjectType.sbyteType);
                        Write((sbyte)value);
                        break;

                    case "Int16":
                        Write((byte)ObjectType.int16Type);
                        Write((short)value);
                        break;

                    case "Int32":
                        Write((byte)ObjectType.int32Type);
                        Write((int)value);
                        break;

                    case "Int64":
                        Write((byte)ObjectType.int64Type);
                        Write((long)value);
                        break;

                    case "Char":
                        Write((byte)ObjectType.charType);
                        base.Write((char)value);
                        break;

                    case "String":
                        Write((byte)ObjectType.stringType);
                        base.Write((string)value);
                        break;

                    case "Single":
                        Write((byte)ObjectType.singleType);
                        Write((float)value);
                        break;

                    case "Double":
                        Write((byte)ObjectType.doubleType);
                        Write((double)value);
                        break;

                    case "Decimal":
                        Write((byte)ObjectType.decimalType);
                        Write((decimal)value);
                        break;

                    case "DateTime":
                        Write((byte)ObjectType.dateTimeType);
                        Write((DateTime)value);
                        break;

                    case "Byte[]":
                        Write((byte)ObjectType.byteArrayType);
                        base.Write((byte[])value);
                        break;

                    case "Char[]":
                        Write((byte)ObjectType.charArrayType);
                        base.Write((char[])value);
                        break;

                    default:
                        Write((byte)ObjectType.otherType);
                        new BinaryFormatter().Serialize(BaseStream, value);
                        break;
                }
            } 
        }
        public void WriteString(string str)
        {
            if (str == null)
            {
                Write((byte)ObjectType.nullType);
            }
            else
            {
                Write((byte)ObjectType.stringType);
                base.Write(str);
            }
        }
        #endregion
    }
}