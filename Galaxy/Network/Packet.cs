using UnityEngine;
using System.Collections;
using System.IO;
using MsgPack.Serialization;
using MessagePack;

namespace Gj.Galaxy.Network
{
    internal enum MessageType:byte{
        Open = 0,
        Close,
        Ping,
        Pong,
        Protocol,
        Namespace,
        Speed,
        Application
    }
    internal class Message{
        internal MessageType type;
        internal long time;
        internal Stream reader;
    }
    internal enum DataType:byte{
        Connect = 0,
        Disconnect,
        Event,
        Ack,
        Error,
        Protocol
    }
    internal interface DataPacket {
        void Packet(Stream writer);
    }
    public enum CompressType:byte{
        None,
        Snappy
    }

    [MessagePackObject(keyAsPropertyName: true)]
    internal class AppPacket:DataPacket{
        public string appId;
        public string version;
        public string secret;
        public AppPacket(string appId, string version, string secret){
            this.appId = appId;
            this.version = version;
            this.secret = secret;
        }

        public void Packet(Stream writer)
        {
            //var serialization = SerializationContext.Default.GetSerializer<AppPacket>();
            //serialization.Pack(writer, this);
            MessagePack.MessagePackSerializer.Serialize<AppPacket>(writer, this);
        }

        public static AppPacket Unpack(Stream reader)
        {
            //var serialization = SerializationContext.Default.GetSerializer<AppPacket>();
            //return serialization.Unpack(reader);
            return MessagePack.MessagePackSerializer.Deserialize<AppPacket>(reader);
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    internal class NsData{
        public DataType type;
        public int id;
        public byte[] nsp;
        public object data;


    }

    internal class NsDataArray:DataPacket{
        public NsData[] data;
        public void Packet(Stream writer)
        {
            //var serialization = SerializationContext.Default.GetSerializer<NsData>();
            //serialization.Pack(writer, this);
            MessagePack.MessagePackSerializer.Serialize<NsData[]>(writer, data);
        }

        public static NsData[] Unpack(Stream reader)
        {
            //var serialization = SerializationContext.Default.GetSerializer<NsData>();
            //return serialization.Unpack(reader);
            return MessagePack.MessagePackSerializer.Deserialize<NsData[]>(reader);
        }
    }
}

