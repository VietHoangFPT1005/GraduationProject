using System.ComponentModel.DataAnnotations;

namespace SP25.OJT202.AccountManagement.Domain.Entities
{
    /// <summary>
    /// Represents a object containing a properties for modify in service layer.
    /// </summary>
    public class UserModification
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
    }
}