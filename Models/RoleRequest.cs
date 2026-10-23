
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class RoleRequest
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public int? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }
}
