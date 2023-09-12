using Fcl.Net.Core;

namespace DapperLabs.Flow.Sdk.Fcl
{
    public class FclWalletProvider
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public FclServiceMethod Method { get; set; }
        public string Endpoint { get; set; }

        public string Uid { get; set; }
    }
}
