using EWallet.Models;

namespace EWallet.Configuration
{
    public sealed class WalletPresets
    {
        public bool Enable { get; init; }
        
        public bool DropDatabase { get; init; }

        public IEnumerable<Wallet> Wallets { get; init; }
    }
}
