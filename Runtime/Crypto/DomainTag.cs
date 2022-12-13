/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;

namespace DapperLabs.Flow.Sdk.Crypto
{
    /// <summary>
    /// Provides functions for dealing with DomainTags
    /// </summary>
    internal static class DomainTag
    {
        private static byte[] MessageWithDomain(byte[] bytes, byte[] domain)
        {
            return CombineByteArrays(new[]
            {
                domain,
                bytes
            });
        }

        private static byte[] CombineByteArrays(byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        /// <summary>
        /// Adds a User domain tag to a byte array
        /// </summary>
        /// <param name="bytes">The byte[] that should have a User domain tag added</param>
        /// <returns>The input byte[] with a User domain tag added</returns>
        internal static byte[] AddUserDomainTag(byte[] bytes)
        {
            var userTag = Extensions.Pad("FLOW-V0.0-user", 32, false);
            return MessageWithDomain(bytes, userTag);
        }

        /// <summary>
        /// Adds a Transaction domain tag to a byte array
        /// </summary>
        /// <param name="bytes">The byte[] that should have a Transaction domain tag added</param>
        /// <returns>The input byte[] with a Transaction domain tag added</returns>
        internal static byte[] AddTransactionDomainTag(byte[] bytes)
        {
            var domainTag = Extensions.Pad("FLOW-V0.0-transaction", 32, false);
            return MessageWithDomain(bytes, domainTag);
        }
    }
}
