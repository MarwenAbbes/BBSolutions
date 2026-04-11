using BB.Infrastructure.DTO;

namespace BB.Infrastructure.Repositories;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    
}