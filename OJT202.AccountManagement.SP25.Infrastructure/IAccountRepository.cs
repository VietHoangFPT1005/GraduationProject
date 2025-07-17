using Microsoft.AspNetCore.Identity;
using SP25.OJT202.AccountManagement.Domain.Entities;

namespace SP25.OJT202.AccountManagement.Infrastructure
{
    /// <summary>
    /// Interface for account repository operations.
    /// </summary>
    public interface IAccountRepository
    {
        //CURD account
        Task<IdentityResult> CreateAccountAsync(User account);

        Task<IdentityResult> UpdateAccountAsync(User account);

        Task<List<User>> GetAccountsAsync();

        Task<IdentityResult> DeleteAccountAsync(User account);
    }
}
