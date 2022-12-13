using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DapperLabs.FlowSDK.DevWallet")]

namespace DapperLabs.Flow.Sdk.Crypto
{
    internal enum SignatureAlgo
    {
        ECDSA_P256 = 2,
        ECDSA_secp256k1 = 3
    }

    internal enum HashAlgo
    {
        SHA2_256 = 1,
        SHA3_256 = 3
    }
}
