using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Crc32C
{
    class NativeProxy
    {
        public static readonly NativeProxy Instance = new NativeProxy();
        protected NativeProxy()
        {
        }
        public uint Append(uint crc, byte[] input, int offset, int length)
        {
            return AppendInternal(crc, input, offset, length);
        }

        private unsafe uint AppendInternal(uint initial, byte[] input, int offset, int length)
        {
            fixed (byte* ptr = &input[offset])
                return External.crc32c_append(initial, ptr, checked((uint)length));
        }

    }
    class External
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
#else
        [DllImport("snappy")]
#endif
        static public unsafe extern uint crc32c_append(uint crc, byte* input, ulong length);
    }
}
