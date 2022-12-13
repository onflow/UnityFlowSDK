/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using Org.BouncyCastle.Crypto.Digests;
using DapperLabs.Flow.Sdk.Crypto;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    internal class Hasher
    {
        internal static byte[] CalculateHash(byte[] bytes, HashAlgo hashAlgo)
        {
            switch (hashAlgo)
            {
                case HashAlgo.SHA2_256:
                    return HashSha2_256(bytes);
                case HashAlgo.SHA3_256:
                    return HashSha3(bytes, 256);
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashAlgo), hashAlgo, null);
            }
        }

        private static byte[] HashSha2_256(byte[] bytes)
        {
            Sha256Digest myHash = new Sha256Digest();
            myHash.BlockUpdate(bytes, 0, bytes.Length);
            byte[] compArr = new byte[myHash.GetDigestSize()];
            myHash.DoFinal(compArr, 0);
            return compArr;
        }

        private static byte[] HashSha3(byte[] bytes, int bitLength)
        {
            var hashAlgorithm = new Sha3Digest(bitLength);
            var result = new byte[hashAlgorithm.GetDigestSize()];
            hashAlgorithm.BlockUpdate(bytes, 0, bytes.Length);
            hashAlgorithm.DoFinal(result, 0);
            return result;
        }
    }
}
