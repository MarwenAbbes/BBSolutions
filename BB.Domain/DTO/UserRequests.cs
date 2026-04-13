using System.ComponentModel.DataAnnotations;

namespace BB.Domain.DTO;

public class BaseUserRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class CreateUserRequest : BaseUserRequest
{
    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest: BaseUserRequest
{
  
}

public class UserResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
