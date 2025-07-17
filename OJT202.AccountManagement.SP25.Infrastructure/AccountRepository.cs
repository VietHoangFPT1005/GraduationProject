using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SP25.OJT202.AccountManagement.Domain;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;

namespace SP25.OJT202.AccountManagement.Infrastructure
{
    /// <summary>
    /// Provides methods for managing user accounts, including creating, updating, retrieving, and deleting accounts.
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        private AccountManagementContext? _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountRepository(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Create teacher account 
        /// </summary>
        /// <param name="teacher">the teacher object to create</param>
        /// <returns>IdentityResult</returns>
        public async Task<IdentityResult> CreateAccountAsync(User account)
        {
            if (string.IsNullOrEmpty(account.checkPassword))
            {
                throw new ArgumentException("Password is required");
            }

            var result = await _userManager.CreateAsync(account, account.checkPassword);

            if (result.Succeeded)
            {
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Teacher));
                await _userManager.AddToRoleAsync(account, ApplicationRoles.Teacher);
            }
            return result;
        }

        /// <summary>
        /// Updates an existing account.
        /// </summary>
        /// <param name="user">The user object to update.</param>
        /// <returns>IdentityResult</returns>
        public async Task<IdentityResult> UpdateAccountAsync(User user)
        {
            _context = new AccountManagementContext();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        /// <summary>
        /// Retrieves all accounts.
        /// </summary>
        /// <returns>A list of all user accounts.</returns>
        public async Task<List<User>> GetAccountsAsync()
        {
            _context = new AccountManagementContext();

            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Deletes an existing account.
        /// </summary>
        /// <param name="user">The user object to delete.</param>
        /// <returns>IdentityResult</returns>
        public async Task<IdentityResult> DeleteAccountAsync(User user)
        {
            _context = new AccountManagementContext();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

    }
}
