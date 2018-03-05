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
        internal int time;
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

    [MessagePackObject]
    internal class AppPacket:DataPacket{
        //[Key("id")]
        [Key(0)]
        public string appId;
        //[Key("version")]
        [Key(1)]
        public string version;
        //[Key("secret")]
        [Key(2)]
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

    [MessagePackObject]
    internal class NsData{
        //[Key("type")]
        [Key(0)]
        public DataType type;
        //[Key("id")]
        [Key(1)]
        public int id = 0;
        //[Key("nsp")]
        [Key(2)]
        public byte[] nsp = null;
        //[Key("data")]
        [Key(3)]
        public object data = null;


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

