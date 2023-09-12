using System.Threading.Tasks;

namespace DapperLabs.Flow.Sdk.Crypto
{
    public interface IWalletlessOnboarding
    {
        public Task LinkToAccount();
    }
}
