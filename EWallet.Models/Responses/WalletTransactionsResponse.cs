namespace EWallet.Models.Responses
{
    public sealed class WalletTransactionsResponse
    {
        public int Count { get; init; }
        public decimal TotalAmount { get; init; }
        public IEnumerable<Transaction> Transactions { get; init; }
    }
}
