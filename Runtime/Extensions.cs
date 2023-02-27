/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using DapperLabs.Flow.Sdk.Exceptions;

[assembly: InternalsVisibleTo("DapperLabs.FlowSDK.DevWallet"),
           InternalsVisibleTo("DapperLabs.FlowSDK.WalletConnect")]

namespace DapperLabs.Flow.Sdk
{
    internal static class Extensions
    {
        internal static byte[] FromHexToBytes(this string hex)
        {
            try
            {
                hex = hex.RemoveHexPrefix();

                if (hex.IsHexString())
                {
                    return Enumerable.Range(0, hex.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                        .ToArray();
                }

                throw new FlowException("Invalid hex string.");
            }
            catch (Exception exception)
            {
                throw new FlowException("Failed to convert hex to byte[].", exception);
            }
        }

        internal static bool IsHexString(this string str)
        {
            try
            {
                if (str.Length == 0)
                    return false;

                str = RemoveHexPrefix(str);

                Regex regex = new Regex(@"^[0-9a-f]+$");
                return regex.IsMatch(str) && str.Length % 2 == 0;
            }
            catch (Exception exception)
            {
                throw new FlowException("Failed to determine if string is hex.", exception);
            }
        }

        internal static string RemoveHexPrefix(this string hex)
        {
            try
            {
                return hex.Substring(hex.StartsWith("0x") ? 2 : 0);
            }
            catch (Exception exception)
            {
                throw new FlowException("Failed to remove hex prefix", exception);
            }
        }

        internal static byte[] Pad(string tag, int length, bool padLeft = true)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(tag);

            return padLeft ? bytes.PadLeft(length) : bytes.PadRight(length);
        }

        internal static byte[] Pad(byte[] bytes, int length, bool padLeft = true)
        {
            return padLeft ? bytes.PadLeft(length) : bytes.PadRight(length);
        }

        private static byte[] PadLeft(this byte[] bytes, int length)
        {
            if (bytes.Length >= length)
                return bytes;

            byte[] newArray = new byte[length];
            Array.Copy(bytes, 0, newArray, newArray.Length - bytes.Length, bytes.Length);
            return newArray;
        }

        private static byte[] PadRight(this byte[] bytes, int length)
        {
            if (bytes.Length >= length)
                return bytes;

            Array.Resize(ref bytes, length);
            return bytes;
        }
    }
}
