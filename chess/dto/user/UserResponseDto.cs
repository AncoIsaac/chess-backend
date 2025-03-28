namespace chess.dto.user
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
    }
}
