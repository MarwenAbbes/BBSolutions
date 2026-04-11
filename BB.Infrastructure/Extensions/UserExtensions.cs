using BB.Domain.DTO;
using BB.Domain.Entities;


namespace BB.Infrastructure.Extensions
{
    public static class UserExtensions
    {
        public static UserResponse ToResponse(this User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };

        public static User ToEntity(this CreateUserRequest request) => new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = request.Password  // raw -- service will hash
        };

        public static User ToEntity(this UpdateUserRequest request, int id) => new()
        {
            Id= id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = string.Empty  // It will be filled in the service layer if needed, otherwise it will be ignored
        };
    }


}
