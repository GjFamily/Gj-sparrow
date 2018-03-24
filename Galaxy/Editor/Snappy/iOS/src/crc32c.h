#ifndef CRC32C_H
#define CRC32C_H

#if UNITY_METRO
#define EXPORT_API __declspec(dllexport) __stdcall
#elif UNITY_WIN
#define EXPORT_API __declspec(dllexport)
#elif UNITY_OSX
#define EXPORT_API __attribute__((visibility("default")))
#else
#define EXPORT_API
#endif

#if 0
typedef int8_t int8;
typedef uint8_t uint8;
typedef int16_t int16;
typedef uint16_t uint16;
typedef int32_t int32;
typedef uint32_t uint32;
typedef int64_t int64;
typedef uint64_t uint64;
#else
typedef signed char int8;
typedef unsigned char uint8;
typedef short int16;
typedef unsigned short uint16;
typedef int int32;
typedef unsigned int uint32;
typedef long long int64;
typedef unsigned long long uint64;
#endif
#include <stdint.h>
#include <stdio.h>

/*
    Computes CRC-32C using Castagnoli polynomial of 0x82f63b78.
    This polynomial is better at detecting errors than the more common CRC-32 polynomial.
    CRC-32C is implemented in hardware on newer Intel processors.
    This function will use the hardware if available and fall back to fast software implementation.
*/
extern "C" EXPORT_API uint32_t crc32c_append(
    uint32_t crc,               // initial CRC, typically 0, may be used to accumulate CRC from multiple buffers
    const uint8_t *input,       // data to be put through the CRC algorithm
    size_t length);             // length of the data in the input buffer

extern "C" EXPORT_API void crc32c_unittest();

#endif
