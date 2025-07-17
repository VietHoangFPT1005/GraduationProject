using System.ComponentModel.DataAnnotations;

namespace SP25.OJT202.AccountManagement.Domain.Entities
{
    /// <summary>
    /// Represents a teacher in the account service management system 
    /// </summary>
    public class Teacher
    {
        [Required(ErrorMessage = "User name is required.")]
        [MinLength(3, ErrorMessage = "User name must be at least 3 characters long.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public double? Salary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance must be a non-negative value.")]
        public double? Balance { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}