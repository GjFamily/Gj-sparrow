using System;
using System.IO;
using UnityEngine;

namespace Gj.Galaxy.Logic{
    interface SerializeFormatter {
        byte[] Serialize(object customobject);
        object Deserialize(byte[] stream);
        short Size();

    }
    internal static class SerializeTypes
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, SerializeFormatter> typeD;
        static readonly global::System.Collections.Generic.Dictionary<string, SerializeFormatter> symbolD;

        static SerializeTypes()
        {
            typeD = new global::System.Collections.Generic.Dictionary<Type, SerializeFormatter>(4)
            {
                {typeof(Vector2), Vector2SerializeFormatter.instance },
                {typeof(Vector3), Vector3SerializeFormatter.instance },
                {typeof(Quaternion), QuaternionSerializeFormatter.instance },
                {typeof(GamePlayer), PlayerSerializeFormatter.instance },
            };
            symbolD = new global::System.Collections.Generic.Dictionary<string, SerializeFormatter>(4)
            {
                {"2", Vector2SerializeFormatter.instance },
                {"3", Vector3SerializeFormatter.instance },
                {"q", QuaternionSerializeFormatter.instance },
                {"p", PlayerSerializeFormatter.instance },
            };
        }

        internal static SerializeFormatter GetFormatter<T>(){
            return GetFormatter(typeof(T));
        }

        internal static SerializeFormatter GetFormatter(Type t)
        {
            SerializeFormatter formatter;
            if (typeD.TryGetValue(t, out formatter))
            {
                return formatter;
            }

            return null;
        }

        internal static SerializeFormatter GetFormatter(string symbol)
        {
            SerializeFormatter formatter;
            if (symbolD.TryGetValue(symbol, out formatter))
            {
                return formatter;
            }

            return null;
        }
    }
    internal class Vector3SerializeFormatter : SerializeFormatter
    {
        internal static readonly short vector3Length = 3 * 4;
        internal static readonly Vector3SerializeFormatter instance = new Vector3SerializeFormatter();
        public byte[] Serialize(object customobject)
        {
            Vector3 vo = (Vector3)customobject;
            byte[] vector3Byte = new byte[vector3Length];
            BitConverter.GetBytes(vo.x).CopyTo(vector3Byte, 0);
            BitConverter.GetBytes(vo.y).CopyTo(vector3Byte, 4);
            BitConverter.GetBytes(vo.z).CopyTo(vector3Byte, 8);

            return vector3Byte;
        }

        public object Deserialize(byte[] stream)
        {
            Vector3 vo = new Vector3();

            vo.x = BitConverter.ToSingle(stream, 0);
            vo.y = BitConverter.ToSingle(stream, 4);
            vo.z = BitConverter.ToSingle(stream, 8);

            return vo;
        }

        public short Size()
        {
            return vector3Length;
        }
    }
    internal class Vector2SerializeFormatter : SerializeFormatter
    {
        internal static readonly short vector2Length = 2 * 4;
        internal static readonly Vector2SerializeFormatter instance = new Vector2SerializeFormatter();
        public byte[] Serialize(object customobject)
        {
            Vector2 vo = (Vector2)customobject;
            byte[] vector2Byte = new byte[vector2Length];
            BitConverter.GetBytes(vo.x).CopyTo(vector2Byte, 0);
            BitConverter.GetBytes(vo.y).CopyTo(vector2Byte, 4);
            return vector2Byte;
        }

        public object Deserialize(byte[] stream)
        {
            Vector2 vo = new Vector2();
            vo.x = BitConverter.ToSingle(stream, 0);
            vo.y = BitConverter.ToSingle(stream, 4);

            return vo;
        }
        public short Size()
        {
            return vector2Length;
        }
    }

    internal class QuaternionSerializeFormatter : SerializeFormatter
    {
        internal static readonly short quarternionLength = 4 * 4;
        internal static readonly QuaternionSerializeFormatter instance = new QuaternionSerializeFormatter();
        public byte[] Serialize(object customobject)
        {
            Quaternion o = (Quaternion)customobject;
            byte[] quarternionByte = new byte[quarternionLength];
            BitConverter.GetBytes(o.w).CopyTo(quarternionByte, 0);
            BitConverter.GetBytes(o.x).CopyTo(quarternionByte, 4);
            BitConverter.GetBytes(o.y).CopyTo(quarternionByte, 8);
            BitConverter.GetBytes(o.z).CopyTo(quarternionByte, 12);

            return quarternionByte;
        }

        public object Deserialize(byte[] stream)
        {
            Quaternion o = new Quaternion();

            o.w = BitConverter.ToSingle(stream, 0);
            o.x = BitConverter.ToSingle(stream, 4);
            o.y = BitConverter.ToSingle(stream, 8);
            o.z = BitConverter.ToSingle(stream, 12);

            return o;
        }
        public short Size()
        {
            return quarternionLength;
        }
    }

    internal class PlayerSerializeFormatter : SerializeFormatter
    {
        internal static readonly short playerLength = 4;
        internal static readonly PlayerSerializeFormatter instance = new PlayerSerializeFormatter();
        public byte[] Serialize(object customobject)
        {
            string ID = ((GamePlayer)customobject).UserId;

            return System.Text.Encoding.Default.GetBytes(ID);
        }

        public object Deserialize(byte[] stream)
        {
            string ID = BitConverter.ToString(stream, 0);
            return RoomConnect.Room.GetPlayer(ID);
        }

        public short Size()
        {
            return playerLength;
        }

    }

}