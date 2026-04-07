using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Frontend.Models
{
    public class UserManagementViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public int? ClientId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserCreateViewModel
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Operador";

        public int? ClientId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UserStatusUpdateViewModel
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserRoleUpdateViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Operador";
    }

    public class UserResetPasswordViewModel
    {
        public int Id { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
