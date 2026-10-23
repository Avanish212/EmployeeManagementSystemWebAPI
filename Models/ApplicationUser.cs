using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";
    }
}