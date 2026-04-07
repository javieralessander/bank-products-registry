using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Frontend.Models
{
    // 1. Lo que enviamos al Backend (ahora coincide con LoginRequest de tu API)
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario o correo es obligatorio.")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    // 2. Lo que recibimos del Backend (ahora coincide con AuthResponse de tu API)
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public AuthenticatedUserResponse? User { get; set; }
    }

    // El objeto de usuario que viene dentro de la respuesta
    public class AuthenticatedUserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? ClientId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    // (Dejamos el Registro igual por ahora)
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [MaxLength(25, ErrorMessage = "La cédula no debe exceder 25 caracteres.")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [MaxLength(25, ErrorMessage = "El teléfono no debe exceder 25 caracteres.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar tu contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}