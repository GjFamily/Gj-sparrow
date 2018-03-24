/*
  Copyright (c) 2013 - 2014 Mark Adler, Robert Vazan

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the author be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
  claim that you wrote the original software. If you use this software
  in a product, an acknowledgment in the product documentation would be
  appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
  misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

#ifndef _CRT_SECURE_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#endif

#include "crc32c.h"

#define NOMINMAX
//#include <windows.h>

#include <nmmintrin.h>
#include <stdio.h>

#include <random>
#include <algorithm>


typedef const uint8_t *buffer;


#include "generated-constants.cpp"

/* Table-driven software version as a fall-back.  This is about 15 times slower
   than using the hardware instructions.  This assumes little-endian integers,
   as is the case on Intel processors that the assembler code here is for. */
static uint32_t append_table(uint32_t crci, buffer input, size_t length)
{
    buffer next = input;
#ifdef _M_X64
    uint64_t crc;
#else
    uint32_t crc;
#endif

    crc = crci ^ 0xffffffff;
#ifdef _M_X64
    while (length && ((uintptr_t)next & 7) != 0)
    {
        crc = table[0][(crc ^ *next++) & 0xff] ^ (crc >> 8);
        --length;
    }
    while (length >= 16)
    {
        crc ^= *(uint64_t *)next;
        uint64_t high = *(uint64_t *)(next + 8);
        crc = table[15][crc & 0xff]
            ^ table[14][(crc >> 8) & 0xff]
            ^ table[13][(crc >> 16) & 0xff]
            ^ table[12][(crc >> 24) & 0xff]
            ^ table[11][(crc >> 32) & 0xff]
            ^ table[10][(crc >> 40) & 0xff]
            ^ table[9][(crc >> 48) & 0xff]
            ^ table[8][crc >> 56]
            ^ table[7][high & 0xff]
            ^ table[6][(high >> 8) & 0xff]
            ^ table[5][(high >> 16) & 0xff]
            ^ table[4][(high >> 24) & 0xff]
            ^ table[3][(high >> 32) & 0xff]
            ^ table[2][(high >> 40) & 0xff]
            ^ table[1][(high >> 48) & 0xff]
            ^ table[0][high >> 56];
        next += 16;
        length -= 16;
    }
#else
    while (length && ((uintptr_t)next & 3) != 0)
    {
        crc = table[0][(crc ^ *next++) & 0xff] ^ (crc >> 8);
        --length;
    }
    while (length >= 12)
    {
        crc ^= *(uint32_t *)next;
        uint32_t high = *(uint32_t *)(next + 4);
        uint32_t high2 = *(uint32_t *)(next + 8);
        crc = table[11][crc & 0xff]
            ^ table[10][(crc >> 8) & 0xff]
            ^ table[9][(crc >> 16) & 0xff]
            ^ table[8][crc >> 24]
            ^ table[7][high & 0xff]
            ^ table[6][(high >> 8) & 0xff]
            ^ table[5][(high >> 16) & 0xff]
            ^ table[4][high >> 24]
            ^ table[3][high2 & 0xff]
            ^ table[2][(high2 >> 8) & 0xff]
            ^ table[1][(high2 >> 16) & 0xff]
            ^ table[0][high2 >> 24];
        next += 12;
        length -= 12;
    }
#endif
    while (length)
    {
        crc = table[0][(crc ^ *next++) & 0xff] ^ (crc >> 8);
        --length;
    }
    return (uint32_t)crc ^ 0xffffffff;
}

extern "C" EXPORT_API uint32_t crc32c_append(uint32_t crc, buffer input, size_t length)
{
    return append_table(crc, input, length);
}
