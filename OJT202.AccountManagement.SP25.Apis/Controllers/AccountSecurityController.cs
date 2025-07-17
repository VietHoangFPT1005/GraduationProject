using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SP25.OJT202.AccountManagement.Application;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Presentation.Loggers;
using SP25.OJT202.AccountManagement.Presentation.Middlewares;

namespace SP25.OJT202.AccountManagement.Presentation.Controllers
{
    [Route("api/authentication/")]
    [ApiController]
    public class AccountSecurityController : ControllerBase
    {
        // Cache key for storing account list
        private const string AccountsCacheKey = "AccountList";

        private readonly IAccountService _accountService;
        private readonly IAccountSecurityService _accountAuthentication;

        /// <summary>
        /// Constructor for AccountController.
        /// </summary>
        /// <param name="accountService">Service for account operations.</param>
        /// <param name="logger">Logger for logging information.</param>
        /// <param name="cache">Cache for storing data.</param>
        public AccountSecurityController(IAccountService accountService, IAccountSecurityService accountAuthentication)
        {
            _accountService = accountService;
            _accountAuthentication = accountAuthentication;
        }

        #region SignUp
        /// <summary>
        /// Signs up a new student.
        /// </summary>
        /// <param name="student">The student to sign up.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPost("students/sign-up")]
        public async Task<IActionResult> SignUpStudentAsync(Student student)
        {
            var user = new User()
            {
                Email = student.Email
            };

            // Check if an account with the same email already exists
            var existingAccount = await _accountService.FindAccountByEmailAsync(user);
            if (existingAccount?.User != null)
            {
                throw new UserExistException();
            }

            // Sign up the student
            var result = await _accountAuthentication.SignUpStudentAsync(student);
            return Ok(result);
        }
        #endregion

        #region SignIn
        /// <summary>
        /// Signs in a student.
        /// </summary>
        /// <param name="account">The account login details.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpGet("login")]
        public async Task<IActionResult> SignInAsync(AccountLogin account)
        {
            // Attempt to sign in the student
            var result = await _accountAuthentication.SignInAsync(account);
            if (result == null)
            {
                throw new UnauthorizedAccessException();
            }
            return Ok(result);
        }
        #endregion

        #region ResetPassword
        [HttpPost("SendOTP")]
        [Authorize]
        public async Task<ActionResult<bool>> SendOTPAsync(string email)
        {
            return await _accountAuthentication.SendOTPAsync(email);
        }

        [HttpPost("VerifyOTP")]
        [Authorize]
        public async Task<ActionResult<bool>> VerifyOTPAsync(string email, string otp)
        {
            return await _accountAuthentication.VerifyOTPAsync(email, otp);
        }

        [HttpPost("ResetPassword")]
        [Authorize]
        public async Task<ActionResult<bool>> ResetPassword(string email, string newPassword)
        {
            return await _accountAuthentication.ResetPasswordAsync(email, newPassword);
        }
        #endregion
    }
}
