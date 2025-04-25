using System.ComponentModel.DataAnnotations;

namespace API.DTOs.User;

public record LoginDto
{

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}