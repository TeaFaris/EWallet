using EWallet.Models;
using EWallet.Models.Requests;
using EWallet.Models.Responses;
using EWallet.Services.Repositories.WalletRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EWallet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class WalletController : ControllerBase
    {
        readonly IWalletRepository walletRepository;
        public WalletController(IWalletRepository walletRepository)
        {
            this.walletRepository = walletRepository;
        }

        [HttpGet("exists")]
        public async Task<ActionResult<WalletExistsResponse>> Exists()
        {
            var userIdClaim = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return new WalletExistsResponse { Exists = false };
            }

            var user = await walletRepository.GetAsync(userId);

            return new WalletExistsResponse { Exists = user is not null };
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(WalletDepositRequest deposit)
        {
            var userIdClaim = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Wallet with this user id doesn't exists.");
            }

            var wallet = await walletRepository.GetAsync(userId);

            if(wallet is null)
            {
                return Unauthorized("Wallet with this user id doesn't exists.");
            }

            var newTransaction = new Transaction
            {
                Amount = deposit.Amount,
                Issued = DateTime.UtcNow,
                Wallet = wallet
            };

            wallet.Balance += deposit.Amount;
            wallet.Transactions.Add(newTransaction);

            await walletRepository.UpdateAsync(wallet);
            await walletRepository.SaveAsync();

            return Ok();
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<WalletTransactionsResponse>> GetTransactions()
        {
            var userIdClaim = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Wallet with this user id doesn't exists.");
            }

            var wallet = await walletRepository.GetAsync(userId);

            if (wallet is null)
            {
                return Unauthorized("Wallet with this user id doesn't exists.");
            }

            var transactions = wallet
                .Transactions
                .Where(x => x.Issued.Month == DateTime.UtcNow.Month);

            return new WalletTransactionsResponse
            {
                Count = transactions.Count(),
                TotalAmount = transactions.Sum(t => t.Amount),
                Transactions = transactions
            };
        }

        [HttpGet("balance")]
        public async Task<ActionResult<WalletBalanceResponse>> GetBalance()
        {
            var userIdClaim = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Wallet with this user id doesn't exists.");
            }

            var wallet = await walletRepository.GetAsync(userId);

            if (wallet is null)
            {
                return Unauthorized("Wallet with this user id doesn't exists.");
            }

            return new WalletBalanceResponse
            {
                Balance = wallet.Balance
            };
        }
    }
}