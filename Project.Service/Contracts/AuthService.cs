using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.Core.DTOs;
using Project.Core.Interfaces;
using Project.Core.Models;
using Project.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project.Service.Contracts
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration config, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = (await _unitOfWork.UserRepository.GetAllAsync())
                    .FirstOrDefault(u => u.UserName == loginDto.Username);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt for non-existent user: {loginDto.Username}");
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning($"Invalid password attempt for user: {loginDto.Username}");
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                var token = GenerateJwtToken(user);

                _logger.LogInformation($"User {user.UserName} logged in successfully");

                return new AuthResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddDays(7)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                throw;
            }
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            try
            {
                if ((await _unitOfWork.UserRepository.GetAllAsync()).Any(u => u.UserName == user.UserName))
                    throw new ArgumentException("Username already exists");

                CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"New user registered: {user.UserName}");

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
}
