using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Org.BouncyCastle.Utilities;
using SP25.OJT202.AccountManagement.Domain.Entities;
using SP25.OJT202.AccountManagement.Domain.Entities.Response;
using SP25.OJT202.AccountManagement.Infrastructure;
using SWP391.EventFlowerExchange.Domain.Entities;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SP25.OJT202.AccountManagement.Application
{
    /// <summary>
    /// Provides service methods for managing user accounts, including sign up, sign in, creating, updating,...
    /// </summary>
    public class AccountSecurityService : IAccountSecurityService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmtpSetting _smtpSetting;


        private static int PAGE_SIZE { get; set; } = 5;

        public AccountSecurityService(UserManager<User> userManager, IOptionsMonitor<SmtpSetting> smtpSetting
            , IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _smtpSetting = smtpSetting.CurrentValue;
        
        }

        #region SignUp

        /// <summary>
        /// Sign up student 
        /// </summary>
        /// <param name="student">the student object to sign up</param>
        /// <returns>IdentityResult</returns>
        public async Task<StatusResponse?> SignUpStudentAsync(Student student)
        {
            if (string.IsNullOrEmpty(student.Password))
            {
                return new StatusResponse()
                {
                    Status = "Failed",
                    Message = "Password is required."
                };
            }

            var user = new User
            {
                UserName = student.UserName,
                Email = student.Email,
                EmailConfirmed = true,
                PhoneNumber = student.PhoneNumber,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTimeOffset.Now,
                Address = student.Address
            };

            var result = await _userManager.CreateAsync(user, student.Password);

            if (result.Succeeded)
            {
                //gan role cho customer
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Student));
                await _userManager.AddToRoleAsync(user, ApplicationRoles.Student);

                return new StatusResponse()
                {
                    Status = "Succeeded",
                    Message = "Sign up student successfully."
                };
            }
            else
                return new StatusResponse()
                {
                    Status = "Failed",
                    Message = "Sign up student failed."
                };
        }

        #endregion

        #region SignIn

        /// <summary>
        /// Sign in account 
        /// </summary>
        /// <param name="LoginAccount">account to login</param>
        /// <returns>token</returns>
        public async Task<TokenResponse?> SignInAsync(AccountLogin loginAccount)
        {
            if (string.IsNullOrEmpty(loginAccount.Email) || string.IsNullOrEmpty(loginAccount.Password))
            {
                throw new ArgumentException();
            }

            var user = await _userManager.FindByEmailAsync(loginAccount.Email);

            if (user == null)
            {
                return null;
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginAccount.Password);

            if (!passwordValid)
            {
                return null;
            }

            var authenClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, loginAccount.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                authenClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var jwtSecret = _configuration["JWT:Secret"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret is not configured.");
            }

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var tokenDescription = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authenClaims,
                signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(authenKey, SecurityAlgorithms.HmacSha256Signature)
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescription);

            return new TokenResponse { Token = token };
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart()
            {
                Text = message
            };

            var smtpServer = _configuration["SmtpSettings:Server"];
            var smtpPort = _configuration["SmtpSettings:Port"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort))
            {
                throw new InvalidOperationException("SMTP server or port is not configured.");
            }

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task<bool> SendOTPAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var otp = new Random().Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiration = DateTime.Now.AddMinutes(2);
            await _userManager.UpdateAsync(user);

            string subject = "This is your OTP code";
            string message = $"Your OTP code is: {otp}. This OTP is valid in 2 minutes.";
            await SendEmailAsync(email, subject, message);

            return true;
        }

        public async Task<bool> VerifyOTPAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            if (CheckValidOptAsync(user, email, otp))
            {
                return true;
            }

            return false;
        }

        public bool CheckValidOptAsync(User user, string email, string otp)
        {
            if (user.OtpCode == otp && user.OtpExpiration > DateTime.Now)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                user.OtpCode = null;
                user.OtpExpiration = null;
                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
        }
        #endregion
    }

}

