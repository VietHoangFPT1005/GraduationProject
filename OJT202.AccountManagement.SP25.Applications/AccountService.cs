using Microsoft.AspNetCore.Identity;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;
using SP25.OJT202.AccountManagement.Infrastructure;

namespace SP25.OJT202.AccountManagement.Application
{
    /// <summary>
    /// Provides service methods for managing user accounts, including sign up, sign in, creating, updating,...
    /// </summary>
    public class AccountService : IAccountService
    {
        private IAccountRepository _repo;
        private readonly UserManager<User> _userManager;

        private static int PAGE_SIZE { get; set; } = 5;

        public AccountService(IAccountRepository repo, UserManager<User> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        #region CURD

        public async Task<StatusResponse?> CreateTeacherAccountAsync(Teacher teacher)

        {
            var user = new User
            {
                Email = teacher.Email,
                EmailConfirmed = true,
                UserName = teacher.UserName,
                PhoneNumber = teacher.PhoneNumber,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTimeOffset.Now,
                Address = teacher.Address,
                Balance = teacher.Balance,
                checkPassword = teacher.Password
            };

            var result = await _repo.CreateAccountAsync(user);

            if (result.Succeeded)
            {
                return new StatusResponse()
                {
                    Status = "Succeeded",
                    Message = "Create teacher account successfully."
                };
            }
            else
            {
                return new StatusResponse()
                {
                    Status = "Failed",
                    Message = "Create teacher account failed."
                };
            }
        }

        public async Task<StatusResponse?> UpdateAccountAsync(UserModification account)

        {
            var user = new User()
            {
                Email = account.Email
            };

            var existingAccount = await this.FindAccountByEmailAsync(user);
            if (existingAccount == null)
            {
                return new StatusResponse()
                {
                    Message = "Account not found.",
                    Status = "Failed"
                };
            }

            existingAccount.User!.UserName = account.UserName;
            existingAccount.User!.Email = account.Email;
            existingAccount.User!.PhoneNumber = account.PhoneNumber;
            if (account.Salary != null || account.Salary == 0)
            {
                existingAccount.User!.Salary = account.Salary;
            }
            if (account.Balance != null || account.Balance == 0)
            {
                existingAccount.User!.Balance = account.Balance;
            }
            existingAccount.User!.Address = account.Address;

            var result = await _repo.UpdateAccountAsync(existingAccount.User!);

            if (result.Succeeded)
            {
                return new StatusResponse()
                {
                    Status = "Succeeded",
                    Message = "Update account successfully."
                };
            }
            else
            {
                return new StatusResponse()
                {
                    Status = "Failed",
                    Message = "Update account failed."
                };
            }
        }

        /// <summary>
        /// Retrieves an account by its ID.
        /// </summary>
        /// <param name="id">The ID of the account.</param>
        /// <returns>The user object if found, otherwise null.</returns>
        public async Task<ObjectResponse?> FindAccountByIdAsync(User account)

        {
            var list = await _repo.GetAccountsAsync();
            var result = list?.FirstOrDefault(x => x.Id == account.Id);

            return new ObjectResponse() { User = result };
        }

        /// <summary>
        /// Retrieves an account by its Email.
        /// </summary>
        /// <param name="id">The Email of the account.</param>
        /// <returns>The user object if found, otherwise null.</returns>
        public async Task<ObjectResponse?> FindAccountByEmailAsync(User account)
        {
            var list = await _repo.GetAccountsAsync();

            return new ObjectResponse() { User = list?.FirstOrDefault(x => x.Email == account.Email) };
        }

        public async Task<ListResponse?> GetAccountsAsync()
        {
            var list = await _repo.GetAccountsAsync();

            return new ListResponse() { List = list.ToList() };
        }

        /// <summary>
        /// Retrieves all student accounts.
        /// </summary>
        /// <returns>A list of all student accounts.</returns>
        public async Task<ListResponse?> GetStudentAccountsAsync()
        {
            return await GetAccountsByRoleAsync(ApplicationRoles.Student);
        }

        /// <summary>
        /// Retrieves all student accounts by role.
        /// </summary>
        /// <param name="role">The role of the account.</param>
        /// <returns>The list user object if found, otherwise new list.</returns>
        public async Task<ListResponse?> GetAccountsByRoleAsync(string role)
        {
            var accountsResponse = await _repo.GetAccountsAsync();
            var accounts = accountsResponse ?? new List<User>();

            var result = new List<User>();

            foreach (var acc in accounts)
            {
                var userRoles = await _userManager.GetRolesAsync(acc);
                if (CheckRoleExist(userRoles, role))
                {
                    result.Add(acc);
                }
            }

            return result.Count > 0 ? new ListResponse() { List = result.ToList() } : new ListResponse() { List = new List<User>() };
        }

        /// <summary>
        /// Check role exit for account.
        /// </summary>
        /// <param name="userRoles">Roles of the account.</param>
        /// <returns>True if found, otherwise false.</returns>
        public bool CheckRoleExist(IList<string>? userRoles, string roleCheck)
        {
            if (userRoles != null && userRoles.Any(userRole => userRole.Equals(roleCheck, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public async Task<ListResponse?> GetAccountsConfigurationAsync(string? searchName, double? fromSalary
     , double? toSalary, string? sortBy, int page = 1)
        {
            var accountsResponse = await _repo.GetAccountsAsync();
            var accounts = accountsResponse?.AsQueryable() ?? Enumerable.Empty<User>().AsQueryable();

            #region Filtering

            if (!string.IsNullOrEmpty(searchName))
            {
                accounts = accounts?.Where(x => x.UserName != null && x.UserName.Contains(searchName));
            }

            if (fromSalary.HasValue)
            {
                accounts = accounts?.Where(x => x.Salary >= fromSalary);
            }

            if (toSalary.HasValue)
            {
                accounts = accounts?.Where(x => x.Salary <= toSalary);
            }

            #endregion

            #region Sorting

            //Default sort by user's name
            accounts = accounts?.OrderBy(x => x.UserName);
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "UserName_desc":
                        accounts = accounts?.OrderByDescending(x => x.UserName);
                        break;

                    case "Salary_asc":
                        accounts = accounts?.OrderBy(x => x.Salary);
                        break;

                    case "Salary_desc":
                        accounts = accounts?.OrderByDescending(x => x.Salary);
                        break;
                }
            }

            #endregion

            #region Paging

            accounts = accounts?.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE);

            #endregion

            var result = accounts?.Select(x => new User
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                Balance = x.Balance,
                Salary = x.Salary,
                CreatedAt = x.CreatedAt
            }).ToList() ?? new List<User>();

            return new ListResponse() { List = result.ToList() };
        }

        public async Task<StatusResponse?> DeleteAccountAsync(User account)

        {
            var result = await _repo.DeleteAccountAsync(account);
            if (result.Succeeded)
                return new StatusResponse()
                {
                    Status = "Succeeded",
                    Message = "Delete account successfully."
                };
            else
                return new StatusResponse()
                {
                    Status = "Failed",
                    Message = "Delete account failed."
                };
        }

        #endregion

    }

}

