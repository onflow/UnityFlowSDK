using System;
using System.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using DapperLabs.Flow.Sdk.Exceptions;

namespace DapperLabs.Flow.Sdk.Crypto
{
    internal class CryptoUtils
    {
        internal static AsymmetricCipherKeyPair GenerateKeyPair(SignatureAlgo signatureAlgo = SignatureAlgo.ECDSA_P256)
        {
            while (true)
            {
                var curveName = SignatureAlgorithmCurveName(signatureAlgo);

                var curve = ECNamedCurveTable.GetByName(curveName);
                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                var secureRandom = new SecureRandom();
                var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

                var generator = new ECKeyPairGenerator("ECDSA");
                generator.Init(keyParams);
                var key = generator.GenerateKeyPair();

                if (DecodePublicKeyToHex(key).Length != 128)
                    continue;

                return key;
            }
        }

        internal static string SignatureAlgorithmCurveName(SignatureAlgo signatureAlgo)
        {
            switch (signatureAlgo)
            {
                case SignatureAlgo.ECDSA_P256:
                    return "P-256";
                case SignatureAlgo.ECDSA_secp256k1:
                    return "secp256k1";
                default:
                    throw new ArgumentOutOfRangeException(nameof(signatureAlgo), signatureAlgo, null);
            }
        }

        internal static string DecodePublicKeyToHex(AsymmetricCipherKeyPair keyPair)
        {
            if (!(keyPair.Public is ECPublicKeyParameters publicKey))
                throw new FlowException("Public key not valid.");

            var pubKeyX = publicKey.Q.XCoord.ToBigInteger().ToByteArrayUnsigned();
            var pubKeyY = publicKey.Q.YCoord.ToBigInteger().ToByteArrayUnsigned();
            byte[] bytes = pubKeyX.Concat(pubKeyY).ToArray();
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        internal static string DecodePrivateKeyToHex(AsymmetricCipherKeyPair keyPair)
        {
            if (!(keyPair.Private is ECPrivateKeyParameters privateKey))
                throw new FlowException("Private key is invalid.");

            byte[] bytes = privateKey.D.ToByteArrayUnsigned();
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
