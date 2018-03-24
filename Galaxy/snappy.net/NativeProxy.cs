using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

namespace Snappy
{
    class NativeProxy
    {
        public static readonly NativeProxy Instance = new NativeProxy();

        public NativeProxy() { }

        public unsafe SnappyStatus Compress(byte* input, int inLength, byte* output, ref int outLength)
        {
            checked
            {
                uint refLength = (uint)outLength;
                var status = External.snappy_compress(input, (uint)inLength, output, ref refLength);
                outLength = (int)refLength;
                return status;
            }
        }

        public unsafe SnappyStatus Uncompress(byte* input, int inLength, byte* output, ref int outLength)
        {
            checked
            {
                uint refLength = (uint)outLength;
                var status = External.snappy_uncompress(input, (uint)inLength, output, ref refLength);
                outLength = (int)refLength;
                return status;
            }
        }

        public int GetMaxCompressedLength(int inLength)
        {
            return checked((int)External.snappy_max_compressed_length((uint)inLength));
        }

        public unsafe SnappyStatus GetUncompressedLength(byte* input, int inLength, out int outLength)
        {
            checked
            {
                uint unsignedLength;
                var status = External.snappy_uncompressed_length(input, (uint)inLength, out unsignedLength);
                outLength = (int)unsignedLength;
                return status;
            }
        }

        public unsafe SnappyStatus ValidateCompressedBuffer(byte* input, int inLength)
        {
            return checked(External.snappy_validate_compressed_buffer(input, (uint)inLength));
        }

        class External
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            [DllImport("__Internal")]
#else
            [DllImport("snappy")]
#endif
            public static unsafe extern SnappyStatus snappy_compress(byte* input, uint input_length, byte* output, ref uint output_length);

#if UNITY_IPHONE && !UNITY_EDITOR
            [DllImport("__Internal")]
#else
            [DllImport("snappy")]
#endif
            public static unsafe extern SnappyStatus snappy_uncompress(byte* input, uint input_length, byte* output, ref uint output_length);

#if UNITY_IPHONE && !UNITY_EDITOR
            [DllImport("__Internal")]
#else
            [DllImport("snappy")]
#endif
            public static extern uint snappy_max_compressed_length(uint input_length);

#if UNITY_IPHONE && !UNITY_EDITOR
            [DllImport("__Internal")]
#else
            [DllImport("snappy")]
#endif
            public static unsafe extern SnappyStatus snappy_uncompressed_length(byte* input, uint input_length, out uint output_length);

#if UNITY_IPHONE && !UNITY_EDITOR
            [DllImport("__Internal")]
#else
            [DllImport("snappy")]
#endif
            public static unsafe extern SnappyStatus snappy_validate_compressed_buffer(byte* input, uint input_length);
        }
    }
}
