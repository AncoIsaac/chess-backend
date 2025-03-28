using chess.Data;
using chess.dto.user;
using chess.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chess.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost]
        public async Task<ActionResult> CreateUser(User userDto)
        {
            if (userDto == null)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);

            if (existingUser != null)
            {
                return Conflict(new { message = "El correo electronico ya existe" });
            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt();

            userDto.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password, salt);

            _context.Users.Add(userDto);
            await _context.SaveChangesAsync();

            var userResponse = new UserResponseDto
            {
                Id = userDto.Id,
                UserName = userDto.UserName,
                Email = userDto.Email,
            };

            var response = new ApiResponse<UserResponseDto>("Usuario creado correctamente", userResponse);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _context.Users.Where(u => !u.IsDeleted).ToListAsync();

            var usersResponse = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
            });

            return Ok(usersResponse);
        }

        [HttpGet("search")]
        public async Task<ActionResult> SearchUsers([FromQuery] string search = "", [FromQuery] int pageSize = 1, [FromQuery] int page = 1)
        {
            var query = _context.Users.AsQueryable();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var users = await _context.Users
                .Where(u => (u.UserName.Contains(search) || u.Email.Contains(search)) && !u.IsDeleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var usersResponse = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
            });

            var response = new
            {
                Data = usersResponse,
                filterValues = new
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                }
            };

            return Ok(response);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, UserPatchDto user)
        {
            var userToUpdate = await _context.Users.FindAsync(id);

            if (userToUpdate is null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Solo actualizar los campos que no sean nulos
            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                userToUpdate.UserName = user.UserName;
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                userToUpdate.Email = user.Email;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new { message = "Error al actualizar usuario", error = ex.Message });
            }

            var userDto = new UserResponseDto
            {
                Id = userToUpdate.Id,
                Email = userToUpdate.Email,
                UserName = userToUpdate.UserName,
            };

            var response = new ApiResponse<UserResponseDto>("Usuario Actualizado correctamente", userDto);


            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null || user.IsDeleted) // También verifica si ya está eliminado
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // En lugar de Remove, actualizamos los campos para soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            // Opcional: registrar qué usuario realizó la eliminación
            user.DeletedBy = user.UserName; // Requiere autenticación

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>("Usuario marcado como eliminado correctamente", ""));
        }
    }
}
