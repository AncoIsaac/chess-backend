using System.ComponentModel.DataAnnotations;

namespace chess.dto.user
{
    public class ForgotPasswordDto
    {
        [Required]
        public required string Email { get; set; }
    }
}
