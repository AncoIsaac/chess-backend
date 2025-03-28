using chess.Data;
using chess.Data.service;
using chess.dto.user;
using chess.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chess.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context, EmailService email) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly EmailService _emailService = email;

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto userLoginDto)
        {
            if (userLoginDto == null)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (user == null || user.IsDeleted)
            {
                return NotFound(new { message = "Usuario no encontrado o eliminado" });
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

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null)
            {
                return BadRequest();
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
            if (user == null || user.IsDeleted)
            {
                return NotFound(new { message = "Usuario no encontrado o eliminado" });
            }

            var code = new Random().Next(100000, 999999).ToString();

            user.CodeVerification = code;
            user.CodeVerificationExpiration = DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(5));
            await _context.SaveChangesAsync();

            await _emailService.SendVerificationEmail(user.Email, code);

            return Ok(new { message = "Código enviado correctamente", code });
        }

        [HttpPost("Reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
            
            if (user == null || user.IsDeleted)
            {
                return NotFound(new { message = "Usuario no encontrado o eliminado" });
            }
            
            if (user.CodeVerification != resetPasswordDto.Code)
            {
                return BadRequest(new { message = "Código incorrecto o expirado" });
            }
            
            if (user.CodeVerificationExpiration < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return BadRequest(new { message = "Código expirado" });
            }

            if (!BCrypt.Net.BCrypt.Verify(resetPasswordDto.NewPassword, user.Password))
            {
                return Unauthorized(new { message = "La contrasena no puede ser igual a la anterior" });

            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword, salt);
            user.CodeVerification = null;
            user.CodeVerificationExpiration = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Contraseña actualizada correctamente" });
        }
    }
}
