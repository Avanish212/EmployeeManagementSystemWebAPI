
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.DTOs
{
    public class CreateRoleRequestDto
    {
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
