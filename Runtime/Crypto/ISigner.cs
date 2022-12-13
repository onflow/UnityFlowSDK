using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DapperLabs.FlowSDK.DevWallet")]

namespace DapperLabs.Flow.Sdk.Crypto
{
    internal interface ISigner
    {
        internal byte[] Sign(byte[] bytes);
    }
}
