using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTOs;
using Project.Core.Enums;
using Project.Core.Models;
using Project.Service.Interfaces;

namespace Project.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized login attempt");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new User
                {
                    UserName = registerDto.Username,
                    Role = registerDto.Role ?? UserRoles.User.ToString()
                };

                var createdUser = await _authService.RegisterAsync(user, registerDto.Password);
                return CreatedAtAction(nameof(Login), new UserDto
                {
                    Username = createdUser.UserName,
                    Role = createdUser.Role
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Registration error");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
