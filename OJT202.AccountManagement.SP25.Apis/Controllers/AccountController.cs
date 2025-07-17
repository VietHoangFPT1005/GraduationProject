using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SP25.OJT202.AccountManagement.Presentation.Middlewares;
using SP25.OJT202.AccountManagement.Application;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Infrastructure;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;
using SP25.OJT202.AccountManagement.Presentation.Loggers;

namespace SP25.OJT202.AccountManagement.Presentation.Controllers
{
    /// <summary>
    /// Account controller operations.
    /// </summary>
    [Route("api/accounts/")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Cache key for storing account list
        private const string AccountsCacheKey = "AccountList";

        private readonly IAccountService _accountService;
        private readonly ConfigurableLogger<AccountController> _logger;
        private IMemoryCache _cache;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Constructor for AccountController.
        /// </summary>
        /// <param name="accountService">Service for account operations.</param>
        /// <param name="logger">Logger for logging information.</param>
        /// <param name="cache">Cache for storing data.</param>
        public AccountController(IAccountService accountService, ConfigurableLogger<AccountController> logger, IMemoryCache cache)
        {
            _accountService = accountService;
            _logger = logger;
            _cache = cache;
        }

        #region CURD
        /// <summary>
        /// Creates a new teacher account.
        /// </summary>
        /// <param name="account">The teacher account details.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPost("teachers/create")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> CreateAccount(Teacher account)
        {
            if (account == null)
            {
                throw new NullReferenceException();
            }

            var user = new User()
            {
                Email = account.Email
            };

            // Check if an account with the same email already exists
            var existingAccount = await _accountService.FindAccountByEmailAsync(user);
            if (existingAccount?.User != null)
            {
                throw new UserExistException();
            }

            // Create the teacher account
            var result = await _accountService.CreateTeacherAccountAsync(account);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing account.
        /// </summary>
        /// <param name="account">The account details to update.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPut("update")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> UpdateAccount(UserModification account)
        {
            if (account == null)
            {
                throw new NullReferenceException();
            }

            // Update the account
            var result = await _accountService.UpdateAccountAsync(account);
            return Ok(result);
        }

        /// <summary>
        /// Searches for an account by ID.
        /// </summary>
        /// <param name="id">The ID of the account to search for.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpGet("search/by-id/{id}")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> SearchAccountById(string id)
        {
            // Find the account by ID
            var existingAccount = await _accountService.FindAccountByIdAsync(new User() { Id = id });
            if (existingAccount == null)
            {
                throw new UserNotFoundException();
            }
            return Ok(existingAccount);
        }

        /// <summary>
        /// Searches for an account by email.
        /// </summary>
        /// <param name="email">The email of the account to search for.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpGet("search/by-email/{email}")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> GetAccountByEmail(string email)
        {
            // Find the account by email
            var existingAccount = await _accountService.FindAccountByEmailAsync(new User() { Email = email });
            if (existingAccount == null)
            {
                throw new UserNotFoundException();
            }
            return Ok(existingAccount);
        }

        /// <summary>
        /// Retrieves all accounts.
        /// </summary>
        /// <returns>An IActionResult containing the list of accounts.</returns>
        [HttpGet()]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> GetAccounts()
        {
            if (_cache.TryGetValue(AccountsCacheKey, out IEnumerable<User>? accounts))
            {
                _logger.LogInformation();
            }
            else
            {
                try
                {
                    await _semaphore.WaitAsync();

                    if (_cache.TryGetValue(AccountsCacheKey, out accounts))
                    {
                        _logger.LogInformation();
                    }
                    else
                    {
                        _logger.LogWarning();

                        var response = await _accountService.GetAccountsAsync();
                        accounts = response?.List;

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(1);

                        _cache.Set(AccountsCacheKey, accounts, cacheEntryOptions);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return Ok(new ListResponse() { List = accounts?.ToList() });
        }

        /// <summary>
        /// Retrieves all student accounts.
        /// </summary>
        /// <returns>An IActionResult containing the list of student accounts.</returns>
        [HttpGet("students")]
        [Authorize(Roles = ApplicationRoles.Teacher)]
        public async Task<IActionResult> GetStudentAccounts()
        {
            if (_cache.TryGetValue(AccountsCacheKey, out IEnumerable<User>? accounts))
            {
                _logger.LogInformation();
            }
            else
            {
                try
                {
                    await _semaphore.WaitAsync();

                    if (_cache.TryGetValue(AccountsCacheKey, out accounts))
                    {
                        _logger.LogInformation();
                    }
                    else
                    {
                        _logger.LogWarning();

                        var response = await _accountService.GetStudentAccountsAsync();
                        accounts = response?.List;

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(1);

                        _cache.Set(AccountsCacheKey, accounts, cacheEntryOptions);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return Ok(accounts);
        }

        /// <summary>
        /// Retrieves accounts by role.
        /// </summary>
        /// <param name="role">The role to filter accounts by.</param>
        /// <returns>An IActionResult containing the list of accounts with the specified role.</returns>
        [HttpGet("role/{role}")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> GetAccountsByRole(string role)
        {
            if (_cache.TryGetValue(AccountsCacheKey, out IEnumerable<User>? accounts))
            {
                _logger.LogInformation();
            }
            else
            {
                try
                {
                    await _semaphore.WaitAsync();

                    if (_cache.TryGetValue(AccountsCacheKey, out accounts))
                    {
                        _logger.LogInformation();
                    }
                    else
                    {
                        _logger.LogWarning();

                        var response = await _accountService.GetAccountsByRoleAsync(role);
                        accounts = response?.List;

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(1);

                        _cache.Set(AccountsCacheKey, accounts, cacheEntryOptions);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return Ok(accounts);
        }

        /// <summary>
        /// Retrieves accounts based on configuration.
        /// </summary>
        /// <param name="search">Search term for filtering accounts.</param>
        /// <param name="fromSalary">Minimum salary for filtering accounts.</param>
        /// <param name="toSalary">Maximum salary for filtering accounts.</param>
        /// <param name="sortBy">Field to sort accounts by.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <returns>An IActionResult containing the list of accounts based on the configuration.</returns>
        [HttpGet("configuration")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> GetAccountsConfiguration(string? search, double? fromSalary, double? toSalary, string? sortBy, int page = 1)
        {
            if (_cache.TryGetValue(AccountsCacheKey, out IEnumerable<User>? accounts))
            {
                _logger.LogInformation();
            }
            else
            {
                try
                {
                    await _semaphore.WaitAsync();

                    if (_cache.TryGetValue(AccountsCacheKey, out accounts))
                    {
                        _logger.LogInformation();
                    }
                    else
                    {
                        _logger.LogWarning();

                        var response = await _accountService.GetAccountsConfigurationAsync(search, fromSalary, toSalary, sortBy, page);
                        accounts = response?.List;

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(1);

                        _cache.Set(AccountsCacheKey, accounts, cacheEntryOptions);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return Ok(accounts);
        }

        /// <summary>
        /// Deletes an account by ID.
        /// </summary>
        /// <param name="id">The ID of the account to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpDelete("delete/by-id/{id}")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteAccountById(string id)
        {
            // Find the account by ID
            var existingAccount = await _accountService.FindAccountByIdAsync(new User() { Id = id });
            if (existingAccount?.User == null)
            {
                throw new UserNotFoundException();
            }

            // Delete the account
            var result = await _accountService.DeleteAccountAsync(existingAccount.User);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an account by email.
        /// </summary>
        /// <param name="email">The email of the account to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpDelete("delete/by-email/{email}")]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteAccountByEmail(string email)
        {
            // Find the account by email
            var existingAccount = await _accountService.FindAccountByEmailAsync(new User() { Email = email });
            if (existingAccount?.User == null)
            {
                throw new UserNotFoundException();
            }

            // Delete the account
            var result = await _accountService.DeleteAccountAsync(existingAccount.User);
            return Ok(result);
        }
        #endregion
    }
}