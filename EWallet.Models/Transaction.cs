using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWallet.Models
{
    public sealed class Transaction
    {
        [Key]
        public int Id { get; init; }

        public int WalletId { get; init; }
        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; init; }

        public DateTime Issued { get; init; }

        public decimal Amount { get; init; }
    }
}
