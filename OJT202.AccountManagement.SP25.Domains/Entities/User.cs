using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP25.OJT202.AccountManagement.Domain.Entities
{
    /// <summary>
    /// Represents a user in the account management system with additional properties such as CreatedAt, Salary, Balance, Address, and Password.
    /// </summary>
    public partial class User : IdentityUser
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public double? Salary { get; set; }

        public double? Balance { get; set; }

        public string? Address { get; set; }

        [NotMapped]
        public string? checkPassword { get; set; }

        public string? OtpCode { get; set; }

        public DateTime? OtpExpiration { get; set; }
    }
}
