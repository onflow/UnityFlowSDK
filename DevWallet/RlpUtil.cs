/*
The MIT License (MIT)

Copyright (c) 2015-2019 Nethereum.com (Juan Blanco) , Logo by Cass (https://github.com/cassiopaia)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or ANY portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    internal static class RlpUtil
    {
        private const int SIZE_THRESHOLD = 56;
        private const byte OFFSET_SHORT_ITEM = 0x80;
        private const byte OFFSET_LONG_ITEM = 0xb7;
        internal const byte OFFSET_SHORT_LIST = 0xc0;
        private const byte OFFSET_LONG_LIST = 0xf7;

        internal static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];
        internal static readonly byte[] ZERO_BYTE_ARRAY = { 0 };

        internal static bool IsNullOrZeroArray(byte[] array)
        {
            return array == null || array.Length == 0;
        }

        internal static bool IsSingleZero(byte[] array)
        {
            return array.Length == 1 && array[0] == 0;
        }

        internal static byte[] EncodeElement(byte[] srcData)
        {
            if (IsNullOrZeroArray(srcData))
                return new[] { OFFSET_SHORT_ITEM };
            if (IsSingleZero(srcData))
                return srcData;
            if (srcData.Length == 1 && srcData[0] < 0x80)
                return srcData;
            if (srcData.Length < SIZE_THRESHOLD)
            {
                // length = 8X
                var length = (byte)(OFFSET_SHORT_ITEM + srcData.Length);
                var data = new byte[srcData.Length + 1];
                Array.Copy(srcData, 0, data, 1, srcData.Length);
                data[0] = length;

                return data;
            }
            else
            {
                // length of length = BX
                // prefix = [BX, [length]]
                var tmpLength = srcData.Length;
                byte byteNum = 0;
                while (tmpLength != 0)
                {
                    ++byteNum;
                    tmpLength = tmpLength >> 8;
                }
                var lenBytes = new byte[byteNum];
                for (var i = 0; i < byteNum; ++i)
                    lenBytes[byteNum - 1 - i] = (byte)(srcData.Length >> (8 * i));
                // first byte = F7 + bytes.length
                var data = new byte[srcData.Length + 1 + byteNum];
                Array.Copy(srcData, 0, data, 1 + byteNum, srcData.Length);
                data[0] = (byte)(OFFSET_LONG_ITEM + byteNum);
                Array.Copy(lenBytes, 0, data, 1, lenBytes.Length);

                return data;
            }
        }

        internal static byte[] EncodeList(params byte[][] items)
        {
            if (items == null || (items.Length == 1 && items[0] == null))
                return new[] { OFFSET_SHORT_LIST };

            var totalLength = 0;
            for (var i = 0; i < items.Length; i++)
                totalLength += items[i].Length;

            byte[] data;

            int copyPos;

            if (totalLength < SIZE_THRESHOLD)
            {
                var dataLength = 1 + totalLength;
                data = new byte[dataLength];

                //single byte length
                data[0] = (byte)(OFFSET_SHORT_LIST + totalLength);
                copyPos = 1;
            }
            else
            {
                // length of length = BX
                // prefix = [BX, [length]]
                var tmpLength = totalLength;
                byte byteNum = 0;

                while (tmpLength != 0)
                {
                    ++byteNum;
                    tmpLength = tmpLength >> 8;
                }

                tmpLength = totalLength;

                var lenBytes = new byte[byteNum];
                for (var i = 0; i < byteNum; ++i)
                    lenBytes[byteNum - 1 - i] = (byte)(tmpLength >> (8 * i));
                // first byte = F7 + bytes.length
                data = new byte[1 + lenBytes.Length + totalLength];

                data[0] = (byte)(OFFSET_LONG_LIST + byteNum);
                Array.Copy(lenBytes, 0, data, 1, lenBytes.Length);

                copyPos = lenBytes.Length + 1;
            }

            //Combine all elements
            foreach (var item in items)
            {
                Array.Copy(item, 0, data, copyPos, item.Length);
                copyPos += item.Length;
            }
            return data;
        }
    }

    internal static class ConvertorForRLPEncodingExtensions
    {
        internal static BigInteger ToBigIntegerFromRLPDecoded(this byte[] bytes)
        {
            if (bytes == null) return 0;
            if (BitConverter.IsLittleEndian)
            {
                var listEncoded = bytes.ToList();
                listEncoded.Insert(0, 0x00);
                bytes = listEncoded.ToArray().Reverse().ToArray();
                return new BigInteger(bytes);
            }
            return new BigInteger(bytes);
        }

        internal static byte[] ToBytesForRLPEncoding(this BigInteger bigInteger)
        {
            return ToBytesFromNumber(bigInteger.ToByteArray());
        }

        internal static byte[] ToBytesForRLPEncoding(this int number)
        {
            return ToBytesFromNumber(BitConverter.GetBytes(number));
        }

        internal static byte[] ToBytesForRLPEncoding(this long number)
        {
            return ToBytesFromNumber(BitConverter.GetBytes(number));
        }

        internal static byte[] ToBytesForRLPEncoding(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        internal static byte[][] ToBytesForRLPEncoding(this string[] strings)
        {
            var output = new List<byte[]>();
            foreach (var str in strings)
                output.Add(str.ToBytesForRLPEncoding());
            return output.ToArray();
        }

        internal static int ToIntFromRLPDecoded(this byte[] bytes)
        {
            return (int)ToBigIntegerFromRLPDecoded(bytes);
        }

        internal static long ToLongFromRLPDecoded(this byte[] bytes)
        {
            return (long)ToBigIntegerFromRLPDecoded(bytes);
        }

        internal static string ToStringFromRLPDecoded(this byte[] bytes)
        {
            if (bytes == null) return "";
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        internal static byte[] ToBytesFromNumber(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            return TrimZeroBytes(bytes);
        }

        internal static byte[] TrimZeroBytes(this byte[] bytes)
        {
            var trimmed = new List<byte>();
            var previousByteWasZero = true;

            for (var i = 0; i < bytes.Length; i++)
            {
                if (previousByteWasZero && bytes[i] == 0)
                    continue;

                previousByteWasZero = false;
                trimmed.Add(bytes[i]);
            }

            return trimmed.ToArray();
        }
    }
}