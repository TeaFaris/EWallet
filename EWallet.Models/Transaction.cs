using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EWallet.Models
{
    public sealed class Transaction
    {
        [Key]
        public int Id { get; init; }

        public int WalletId { get; init; }
        [ForeignKey(nameof(WalletId))]
        [JsonIgnore]
        public Wallet Wallet { get; init; }

        public DateTime Issued { get; init; }

        public decimal Amount { get; init; }
    }
}
