using chess.Data;
using chess.dto.user;
using chess.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chess.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto userLoginDto)
        {
            if (userLoginDto == null)
            {
                return BadRequest();
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
         
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
            {
                return Unauthorized(new { message = "no se pudo" });
                
            }

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
            };

            var response = new ApiResponse<UserResponseDto>("Usuario logueado correctamente", userDto);

            return Ok(response);
        }
    }
}
