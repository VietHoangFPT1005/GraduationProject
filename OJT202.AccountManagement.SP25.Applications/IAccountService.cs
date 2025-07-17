using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;

namespace SP25.OJT202.AccountManagement.Application
{
    /// <summary>
    /// Interface for account service operations.
    /// </summary>
    public interface IAccountService
    {

        //CURD account
        Task<StatusResponse?> CreateTeacherAccountAsync(Teacher teacher);

        Task<StatusResponse?> UpdateAccountAsync(UserModification account);

        Task<ObjectResponse?> FindAccountByIdAsync(User account);

        Task<ObjectResponse?> FindAccountByEmailAsync(User account);

        Task<ListResponse?> GetAccountsAsync();

        Task<ListResponse?> GetStudentAccountsAsync();

        Task<ListResponse?> GetAccountsConfigurationAsync(string? searchName
            , double? fromSalary, double? toSalary, string? sortBy, int page);

        Task<ListResponse?> GetAccountsByRoleAsync(string role);

        Task<StatusResponse?> DeleteAccountAsync(User account);
    }
}