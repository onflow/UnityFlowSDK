/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using DapperLabs.Flow.Sdk.Crypto;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    internal class Signer : ISigner
    {
        private ECPrivateKeyParameters PrivateKey { get; }
        private HashAlgo HashAlgo { get; }
        private string SignatureCurveName { get; }

        internal Signer(ECPrivateKeyParameters privateKey, HashAlgo hashAlgorithm, SignatureAlgo signatureAlgo)
        {
            PrivateKey = privateKey;
            HashAlgo = hashAlgorithm;
            SignatureCurveName = CryptoUtils.SignatureAlgorithmCurveName(signatureAlgo);
        }

        internal Signer(string privateKey, HashAlgo hashAlgorithm, SignatureAlgo signatureAlgo)
        {
            PrivateKey = Utilities.GeneratePrivateKeyFromHex(privateKey, signatureAlgo);
            HashAlgo = hashAlgorithm;
            SignatureCurveName = CryptoUtils.SignatureAlgorithmCurveName(signatureAlgo);
        }

        byte[] ISigner.Sign(byte[] bytes)
        {
            var curve = ECNamedCurveTable.GetByName(SignatureCurveName);
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            byte[] pkeyBytes = PrivateKey.D.ToByteArrayUnsigned();
            string pkeyStr = BitConverter.ToString(pkeyBytes).Replace("-", "").ToLower();
            var keyParameters = new ECPrivateKeyParameters(new BigInteger(pkeyStr, 16), domain);

            var hash = Hasher.CalculateHash(bytes, HashAlgo);

            var signer = new ECDsaSigner();
            signer.Init(true, keyParameters);

            var output = signer.GenerateSignature(hash);

            var r = output[0].ToByteArrayUnsigned();
            var s = output[1].ToByteArrayUnsigned();

            var rSig = new byte[32];
            Array.Copy(r, 0, rSig, rSig.Length - r.Length, r.Length);

            var sSig = new byte[32];
            Array.Copy(s, 0, sSig, sSig.Length - s.Length, s.Length);

            return rSig.Concat(sSig).ToArray();
        }
    }
}
