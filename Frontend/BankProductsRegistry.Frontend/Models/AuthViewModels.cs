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
        public AuthenticatedUserResponse User { get; set; }
    }

    // El objeto de usuario que viene dentro de la respuesta
    public class AuthenticatedUserResponse
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
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

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar tu contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}