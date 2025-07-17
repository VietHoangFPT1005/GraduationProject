using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;

namespace SP25.OJT202.AccountManagement.Application
{
    /// <summary>
    /// Interface for account authentication operations.
    /// </summary>
    public interface IAccountSecurityService
    {
        //SignUp
        Task<StatusResponse?> SignUpStudentAsync(Student student);

        //Login
        Task<TokenResponse?> SignInAsync(AccountLogin loginAccount);

        //Send OTP
        public Task SendEmailAsync(string toEmail, string subject, string message);
        public Task<bool> SendOTPAsync(string email);
        public Task<bool> VerifyOTPAsync(string email, string otp);

        //Reset Password
        public Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
}