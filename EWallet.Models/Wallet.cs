using System.ComponentModel.DataAnnotations;

namespace EWallet.Models
{
    public sealed class Wallet
    {
        [Key]
        public int Id { get; init; }

        public decimal Balance { get; init; }

        public List<Transaction> Transactions { get; init; }
    }
}