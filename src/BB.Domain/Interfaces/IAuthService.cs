using BB.Domain.DTO;

namespace BB.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
