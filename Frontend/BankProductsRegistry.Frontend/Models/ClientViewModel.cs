using System.Text.Json.Serialization;

namespace BankProductsRegistry.Frontend.Models
{
    public class ClientViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        [JsonIgnore]
        public int? LinkUserId { get; set; }
    }

    public class PendingClientUserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NationalId { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
