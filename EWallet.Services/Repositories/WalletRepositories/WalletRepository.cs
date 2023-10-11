using EWallet.Data;
using EWallet.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EWallet.Services.Repositories.WalletRepositories
{
    public class WalletRepository : IWalletRepository
    {
        readonly ApplicationDbContext dbContext;

        public WalletRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(Wallet entity)
        {
            await dbContext.Wallets.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<Wallet> entities)
        {
            await dbContext.Wallets.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<Wallet>> FindAsync(Expression<Func<Wallet, bool>> predicate)
        {
            return await dbContext
                .Wallets
                .Include(x => x.Transactions)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Wallet>> GetAllAsync()
        {
            return await dbContext
                .Wallets
                .Include(x => x.Transactions)
                .ToListAsync();
        }

        public async Task<Wallet?> GetAsync(int id)
        {
            return await dbContext
                .Wallets
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task RemoveAsync(Wallet entity)
        {
            dbContext.Wallets.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<Wallet> entities)
        {
            dbContext.Wallets.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task SaveAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(Wallet entity)
        {
            dbContext.Wallets.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Wallet> entities)
        {
            dbContext.Wallets.UpdateRange(entities);
            return Task.CompletedTask;
        }
    }
}
