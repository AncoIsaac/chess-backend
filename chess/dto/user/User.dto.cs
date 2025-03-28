using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace chess.dto.user
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        public required string UserName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }
        [JsonIgnore]
        public string? CodeVerification { get; set; }
        [JsonIgnore]
        public DateOnly? CodeVerificationExpiration { get; set; }
    }
}
