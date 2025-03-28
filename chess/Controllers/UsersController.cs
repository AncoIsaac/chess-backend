using chess.Data;
using chess.dto.user;
using chess.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            userDto.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

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
            var users = await _context.Users.ToListAsync();
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
                .Where(u => u.UserName.Contains(search) || u.Email.Contains(search))
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

    }
}
