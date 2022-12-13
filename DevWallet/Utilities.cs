/*
MIT License

Copyright (c) 2021 Tyron Brand

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Crypto;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    internal class Utilities
    {
        internal static ECPrivateKeyParameters GeneratePrivateKeyFromHex(string privateKeyHex, SignatureAlgo signatureAlgo)
        {
            var curveName = CryptoUtils.SignatureAlgorithmCurveName(signatureAlgo);
            var curve = ECNamedCurveTable.GetByName(curveName);
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            return new ECPrivateKeyParameters(new BigInteger(privateKeyHex, 16), domain);
        }

        internal static AsymmetricCipherKeyPair AsymmetricCipherKeyPairFromPrivateKey(string privateKeyHex, SignatureAlgo signatureAlgo)
        {
            var privateParams = GeneratePrivateKeyFromHex(privateKeyHex, signatureAlgo);
            var privateKeyArray = privateParams.D.ToByteArrayUnsigned();

            var privateKeyInt = new BigInteger(+1, privateKeyArray);

            var curveName = CryptoUtils.SignatureAlgorithmCurveName(signatureAlgo);
            var curve = ECNamedCurveTable.GetByName(curveName);
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            var eCPoint = curve.G.Multiply(privateKeyInt).Normalize();

            var publicKey = new ECPublicKeyParameters(eCPoint, domain);

            return new AsymmetricCipherKeyPair(publicKey, privateParams);
        }

        internal static SignatureAlgo GetSignatureAlgorithm(AsymmetricCipherKeyPair keyPair)
        {
            if (!(keyPair.Public is ECPublicKeyParameters publicKey))
                throw new FlowException("Public key not valid.");

            var ecNamedCurves = ECNamedCurveTable.Names;

            foreach (var name in ecNamedCurves)
            {
                var curve = ECNamedCurveTable.GetByName((string)name);
                if (curve != null
                    && curve.Curve.Equals(publicKey.Parameters.Curve)
                    && curve.G.Equals(publicKey.Parameters.G)
                    && Equals(curve.N, publicKey.Parameters.N)
                    && Equals(curve.H, publicKey.Parameters.H))
                {
                    switch ((string)name)
                    {
                        case "secp256r1":
                            return SignatureAlgo.ECDSA_P256;
                        case "secp256k1":
                            return SignatureAlgo.ECDSA_secp256k1;
                    }
                }
            }

            throw new FlowException("Failed to find signature algorithm");
        }

        internal static Crypto.ISigner CreateSigner(string privateKeyHex, SignatureAlgo signatureAlgo, HashAlgo hashAlgo)
        {
            var privateKeyParams = GeneratePrivateKeyFromHex(privateKeyHex, signatureAlgo);
            return new Signer(privateKeyParams, hashAlgo, signatureAlgo);
        }
    }
}
