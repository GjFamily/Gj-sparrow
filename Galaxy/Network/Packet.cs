using UnityEngine;
using System.Collections;
using System.IO;
using MessagePack;
using System;

namespace Gj.Galaxy.Network
{
	public class Message{
        internal byte code;
        internal ProtocolConn conn;
        internal Stream reader;
        internal Action<Action<Stream>> GetReader;
        internal void Close()
        {
            conn.Release();
        }      

        public string ReadString()
        {
            string result = "";
            this.GetReader((reader) =>
            {
                result = new StreamReader(reader).ReadToEnd();
            });
            return result;
        }
        
        public byte[] ReadBytes()
		{
			byte[] bytes = null;
			this.GetReader((reader) =>
            {
				bytes = new byte[reader.Length];
				reader.Read(bytes, 0, bytes.Length);
            });
			return bytes;
		}

        public object[] ReadObject()
		{
			object[] objects = null;
            this.GetReader((reader) =>
            {
				objects = MessagePack.MessagePackSerializer.Deserialize<object[]>(reader);
            });
			return objects;
		}

		public Stream Reader()
		{
			Stream result = null;
			this.GetReader((reader) =>
			{
				result = reader;
            });
			return result;
		}
    }
    public enum DataType:byte{
        Connect = 0,
        Disconnect,
        Event,
        Ack,
        Error,
        Protocol
    }
    public interface DataPacket {
        void Packet(Stream writer);
    }
    public enum CompressType:byte{
        None,
        Snappy
    }

    [MessagePackObject]
    public class AppPacket:DataPacket{
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
            MessagePack.MessagePackSerializer.Serialize<AppPacket>(writer, this);
        }

        public static AppPacket Unpack(Stream reader)
        {
            return MessagePack.MessagePackSerializer.Deserialize<AppPacket>(reader);
        }
    }

    [MessagePackObject]
    public class NsData{
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

    public class NsDataArray:DataPacket{
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

