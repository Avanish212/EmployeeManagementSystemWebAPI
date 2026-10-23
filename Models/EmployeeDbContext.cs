using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Models
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
            : base(options)
        {
        }
        public DbSet<RoleRequest> RoleRequests { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
    }
}