using System.ComponentModel.DataAnnotations;

namespace chess.dto.user
{
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Code { get; set; }

        [Required]
        public required string NewPassword { get; set; }
    }
}
