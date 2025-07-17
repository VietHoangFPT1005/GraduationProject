using System.ComponentModel.DataAnnotations;

namespace SP25.OJT202.AccountManagement.Domain.Entities
{
    /// <summary>
    /// Represents the login credentials for an account, including Email and Password.
    /// </summary>
    public class AccountLogin
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }
    }
}