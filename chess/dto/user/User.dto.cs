using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace chess.dto.user
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        
        [Required]
        public required string UserName { get; set; }

        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        [Required]
        public required string Password { get; set; }

        [JsonIgnore]
        public string? CodeVerification { get; set; }

        [JsonIgnore]
        public DateOnly? CodeVerificationExpiration { get; set; }

        // Campos nuevos para soft delete
        [JsonIgnore]
        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]
        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public string? DeletedBy { get; set; }
    }
}