using System;
using System.IO;
using UnityEngine;

namespace Gj.Galaxy.Logic{
    internal static class SerializeTypes
    {
        #region Custom De/Serializer Methods

        private static readonly byte[] memVector3 = new byte[3 * 4];
        internal static short SerializeVector3(Stream outStream, object customobject)
        {
            Vector3 vo = (Vector3)customobject;
            outStream.Write(BitConverter.GetBytes(vo.x), 0, 4);
            outStream.Write(BitConverter.GetBytes(vo.y), 0, 4);
            outStream.Write(BitConverter.GetBytes(vo.z), 0, 4);

            return 3 * 4;
        }

        internal static object DeserializeVector3(Stream inStream, short length)
        {
            Vector3 vo = new Vector3();

            lock (memVector3)
            {
                inStream.Read(memVector3, 0, 3 * 4);
                vo.x = BitConverter.ToSingle(memVector3, 0);
                vo.y = BitConverter.ToSingle(memVector3, 4);
                vo.z = BitConverter.ToSingle(memVector3, 8);
            }

            return vo;
        }


        private static readonly byte[] memVector2 = new byte[2 * 4];
        internal static short SerializeVector2(Stream outStream, object customobject)
        {
            Vector2 vo = (Vector2)customobject;

            outStream.Write(BitConverter.GetBytes(vo.x), 0, 4);
            outStream.Write(BitConverter.GetBytes(vo.y), 0, 4);
            return 2 * 4;
        }

        internal static object DeserializeVector2(Stream inStream, short length)
        {
            Vector2 vo = new Vector2();
            lock (memVector2)
            {
                inStream.Read(memVector2, 0, 2 * 4);
                vo.x = BitConverter.ToSingle(memVector3, 0);
                vo.y = BitConverter.ToSingle(memVector3, 4);
            }

            return vo;
        }


        private static readonly byte[] memQuarternion = new byte[4 * 4];
        internal static short SerializeQuaternion(Stream outStream, object customobject)
        {
            Quaternion o = (Quaternion)customobject;
            outStream.Write(BitConverter.GetBytes(o.w), 0, 4);
            outStream.Write(BitConverter.GetBytes(o.x), 0, 4);
            outStream.Write(BitConverter.GetBytes(o.y), 0, 4);
            outStream.Write(BitConverter.GetBytes(o.z), 0, 4);

            return 4 * 4;
        }

        internal static object DeserializeQuaternion(Stream inStream, short length)
        {
            Quaternion o = new Quaternion();

            lock (memQuarternion)
            {
                inStream.Read(memQuarternion, 0, 4 * 4);
                o.w = BitConverter.ToSingle(memQuarternion, 0);
                o.x = BitConverter.ToSingle(memQuarternion, 4);
                o.y = BitConverter.ToSingle(memQuarternion, 8);
                o.z = BitConverter.ToSingle(memQuarternion, 12);
            }

            return o;
        }

        private static readonly byte[] memPlayer = new byte[4];
        internal static short SerializePlayer(Stream outStream, object customobject)
        {
            int ID = ((NetworkPlayer)customobject).Id;

            outStream.Write(BitConverter.GetBytes(ID), 0, 4);
            return 4;
        }

        internal static object DeserializePlayer(Stream inStream, short length)
        {
            int ID;
            lock (memPlayer)
            {
                inStream.Read(memPlayer, 0, length);
                ID = BitConverter.ToInt32(memPlayer, 0);
            }
            return GameConnect.Room.GetPlayerWithId(ID);
        }

        #endregion
    }

}