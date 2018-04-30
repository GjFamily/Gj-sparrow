#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, object> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, object>(8)
            {
                {typeof(global::Gj.Galaxy.Network.DataType), new MessagePack.Formatters.Gj.Galaxy.Network.DataTypeFormatter() },
                {typeof(global::Gj.Galaxy.Network.AppPacket), new MessagePack.Formatters.Gj.Galaxy.Network.AppPacketFormatter() },
                {typeof(global::Gj.Galaxy.Network.NsData), new MessagePack.Formatters.Gj.Galaxy.Network.NsDataFormatter() },
                {typeof(global::Gj.Galaxy.Network.NsData[]), new MessagePack.Formatters.ArrayFormatter<global::Gj.Galaxy.Network.NsData>() },
                {typeof(global::System.Collections.Generic.Dictionary<byte, object>), new MessagePack.Formatters.DictionaryFormatter<byte, object>() },
                {typeof(global::System.Collections.Generic.Dictionary<string, object>), new MessagePack.Formatters.DictionaryFormatter<string, object>() },
                {typeof(global::System.Collections.Generic.List<object>), new MessagePack.Formatters.ListFormatter<object>() },
                {typeof(global::System.Collections.Generic.List<object[]>), new MessagePack.Formatters.ListFormatter<object[]>() },
                {typeof(global::System.Object[]), new MessagePack.Formatters.ArrayFormatter<object>() },
                //{typeof(global::System.Collections.Hashtable), (object)MessagePack.Formatters.PrimitiveObjectFormatter.Instance },
            };
        }

        internal static object GetFormatter(Type t)
        {
            object formatter;
            if (lookup.TryGetValue(t, out formatter))
            {
                return formatter;
            }

            return null;
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.Gj.Galaxy.Network
{
    using System;
    using MessagePack;

    public sealed class DataTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Gj.Galaxy.Network.DataType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Gj.Galaxy.Network.DataType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteByte(ref bytes, offset, (Byte)value);
        }

        public global::Gj.Galaxy.Network.DataType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Gj.Galaxy.Network.DataType)MessagePackBinary.ReadByte(bytes, offset, out readSize);
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612


#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.Gj.Galaxy.Network
{
    using System;
    using MessagePack;


    public sealed class AppPacketFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Gj.Galaxy.Network.AppPacket>
    {

        public int Serialize(ref byte[] bytes, int offset, global::Gj.Galaxy.Network.AppPacket value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.appId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.version, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.secret, formatterResolver);
            return offset - startOffset;
        }

        public global::Gj.Galaxy.Network.AppPacket Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __appId__ = default(string);
            var __version__ = default(string);
            var __secret__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __appId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __version__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __secret__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Gj.Galaxy.Network.AppPacket(__appId__, __version__, __secret__);
            ____result.appId = __appId__;
            ____result.version = __version__;
            ____result.secret = __secret__;
            return ____result;
        }
    }


    public sealed class NsDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Gj.Galaxy.Network.NsData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::Gj.Galaxy.Network.NsData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<global::Gj.Galaxy.Network.DataType>().Serialize(ref bytes, offset, value.type, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.id);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.nsp, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<object>().Serialize(ref bytes, offset, value.data, formatterResolver);
            return offset - startOffset;
        }

        public global::Gj.Galaxy.Network.NsData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __type__ = default(global::Gj.Galaxy.Network.DataType);
            var __id__ = default(int);
            var __nsp__ = default(byte[]);
            var __data__ = default(object);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __type__ = formatterResolver.GetFormatterWithVerify<global::Gj.Galaxy.Network.DataType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __id__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __nsp__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __data__ = formatterResolver.GetFormatterWithVerify<object>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Gj.Galaxy.Network.NsData();
            ____result.type = __type__;
            ____result.id = __id__;
            ____result.nsp = __nsp__;
            ____result.data = __data__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

//namespace MessagePack.Formatters.Gj.Galaxy.Network
//{
    //using System;
    //using MessagePack;

    //public sealed class HashtableFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Collections.Hashtable>
    //{
    //    public int Serialize(ref byte[] bytes, int offset, global::System.Collections.Hashtable value, global::MessagePack.IFormatterResolver formatterResolver)
    //    {
    //        var startOffset = offset;
    //        offset += MessagePackBinary.WriteMapHeader(ref bytes, offset, value.Count);
    //        foreach (System.Collections.DictionaryEntry item in value)
    //        {
    //            offset += Serialize(ref bytes, offset, item.Key, formatterResolver);
    //            offset += Serialize(ref bytes, offset, item.Value, formatterResolver);
    //        }
    //        return offset - startOffset;
    //    }

    //    public global::System.Collections.Hashtable Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
    //    {
    //        if (MessagePackBinary.IsNil(bytes, offset))
    //        {
    //            readSize = 1;
    //            return default(global::System.Collections.Hashtable);
    //        }
    //        else
    //        {
    //            var startOffset = offset;

    //            var len = MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
    //            offset += readSize;

    //            var dict = Create(len);
    //            for (int i = 0; i < len; i++)
    //            {
    //                var key = GetFormatter().Deserialize(bytes, offset, formatterResolver, out readSize);
    //                offset += readSize;

    //                var value = valueFormatter.Deserialize(bytes, offset, formatterResolver, out readSize);
    //                offset += readSize;

    //                Add(dict, i, key, value);
    //            }
    //            readSize = offset - startOffset;

    //            return Complete(dict);
    //        }
    //    }

    //    public global::System.Collections.Hashtable Create(int len){
    //        return new global::System.Collections.Hashtable(len);
    //    }

    //    public void Add(global::System.Collections.Hashtable obj, int index, object key, object value){
    //        obj.Add(key, value);
    //    }

    //    public global::System.Collections.Hashtable Complete(global::System.Collections.Hashtable obj){
    //        return obj;
    //    }

    //    public global::MessagePack.Formatters.IMessagePackFormatter GetFormatter(object value){
            
    //    }

    //}


//}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612