using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BB.Infrastructure.DTO;
using BB.Infrastructure.Models;
using BB.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BB.Infrastructure.Repositories;

public class AuthService  : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository repo, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _repo           = repo;
        _passwordHasher = passwordHasher;
        _configuration  = configuration;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _repo.GetByEmailAsync(request.Email);
        if (user is null) return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateToken(user);
        return new AuthResponse
        {
            Token     = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:ExpiryMinutes"]!))
        };
    }

    private string GenerateToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}