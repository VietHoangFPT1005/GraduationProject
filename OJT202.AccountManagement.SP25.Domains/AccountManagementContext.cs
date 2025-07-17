using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SP25.OJT202.AccountManagement.Domain.Entities;

namespace SP25.OJT202.AccountManagement.Domain
{
    /// <summary>
    /// Represents the database context for account management, including user and identity management.
    /// </summary>
    public partial class AccountManagementContext : IdentityDbContext<User>
    {
        public AccountManagementContext()
        {
        }

        public AccountManagementContext(DbContextOptions<AccountManagementContext> options)
            : base(options)
        {
        }

        public new DbSet<User> Users { get; set; }

        // Get connection string from appsettings.json
        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
            var strConn = config["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(strConn))
            {
                throw new Exception("Connection string not found");
            }
            return strConn;
        }

        // Configure the database connection
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());

        // Configure the model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(iul => new { iul.LoginProvider, iul.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(iur => new { iur.UserId, iur.RoleId });
            modelBuilder.Entity<IdentityUserToken<string>>().HasKey(iut => new { iut.UserId, iut.LoginProvider, iut.Name });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}