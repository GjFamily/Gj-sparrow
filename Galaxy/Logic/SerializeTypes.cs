using System;
using System.IO;
using UnityEngine;

namespace Gj.Galaxy.Logic{
    interface SerializeFormatter {
        short Serialize(Stream outStream, object customobject);
        object Deserialize(byte[] stream);
        short Length();
    }
    internal static class SerializeTypes
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, SerializeFormatter> lookup;

        static SerializeTypes()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, SerializeFormatter>(4)
            {
                {typeof(Vector2), Vector2SerializeFormatter.instance },
                {typeof(Vector3), Vector3SerializeFormatter.instance },
                {typeof(Quaternion), QuaternionSerializeFormatter.instance },
                {typeof(NetworkPlayer), PlayerSerializeFormatter.instance },
            };
        }

        internal static SerializeFormatter GetFormatter<T>(){
            return GetFormatter(typeof(T));
        }

        internal static SerializeFormatter GetFormatter(Type t)
        {
            SerializeFormatter formatter;
            if (lookup.TryGetValue(t, out formatter))
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
        public short Serialize(Stream outStream, object customobject)
        {
            Vector3 vo = (Vector3)customobject;
            outStream.Write(BitConverter.GetBytes(vo.x), 0, 4);
            outStream.Write(BitConverter.GetBytes(vo.y), 0, 4);
            outStream.Write(BitConverter.GetBytes(vo.z), 0, 4);

            return 3 * 4;
        }

        public object Deserialize(byte[] stream)
        {
            Vector3 vo = new Vector3();

            vo.x = BitConverter.ToSingle(stream, 0);
            vo.y = BitConverter.ToSingle(stream, 4);
            vo.z = BitConverter.ToSingle(stream, 8);

            return vo;
        }

        public short Length()
        {
            return vector3Length;
        }
    }
    internal class Vector2SerializeFormatter : SerializeFormatter
    {
        internal static readonly short vector2Length = 2 * 4;
        internal static readonly Vector2SerializeFormatter instance = new Vector2SerializeFormatter();
        public short Serialize(Stream outStream, object customobject)
        {
            Vector2 vo = (Vector2)customobject;

            outStream.Write(BitConverter.GetBytes(vo.x), 0, 4);
            outStream.Write(BitConverter.GetBytes(vo.y), 0, 4);
            return 2 * 4;
        }

        public object Deserialize(byte[] stream)
        {
            Vector2 vo = new Vector2();
            vo.x = BitConverter.ToSingle(stream, 0);
            vo.y = BitConverter.ToSingle(stream, 4);

            return vo;
        }
        public short Length()
        {
            return vector2Length;
        }
    }

    internal class QuaternionSerializeFormatter : SerializeFormatter
    {
        internal static readonly short quarternionLength = 4 * 4;
        internal static readonly QuaternionSerializeFormatter instance = new QuaternionSerializeFormatter();
        public short Serialize(Stream outStream, object customobject)
        {
            Quaternion o = (Quaternion)customobject;
            outStream.Write(BitConverter.GetBytes(o.w), 0, 4);
            outStream.Write(BitConverter.GetBytes(o.x), 0, 4);
            outStream.Write(BitConverter.GetBytes(o.y), 0, 4);
            outStream.Write(BitConverter.GetBytes(o.z), 0, 4);

            return 4 * 4;
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
        public short Length()
        {
            return quarternionLength;
        }
    }

    internal class PlayerSerializeFormatter : SerializeFormatter
    {
        internal static readonly short playerLength = 4;
        internal static readonly PlayerSerializeFormatter instance = new PlayerSerializeFormatter();
        public short Serialize(Stream outStream, object customobject)
        {
            int ID = ((NetworkPlayer)customobject).Id;

            outStream.Write(BitConverter.GetBytes(ID), 0, 4);
            return 4;
        }

        public object Deserialize(byte[] stream)
        {
            int ID = BitConverter.ToInt32(stream, 0);
            return GameConnect.Room.GetPlayerWithId(ID);
        }

        public short Length()
        {
            return playerLength;
        }

    }

}