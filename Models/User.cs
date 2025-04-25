using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Utils;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public enum Roles
{
    CUSTOMER,
    SELLER,
    ADMIN
}


[Index(nameof(Email), IsUnique = true)]
public class BaseUser(string FullName, string Email)
{
    [Required]
    public string FullName { get; set; } = FullName;

    [Required, EmailAddress]
    public string Email { get; set; } = Email;
}

public class User(long Id, string FullName, string Email, string Password) : BaseUser(FullName, Email)
{
    public long Id { get; set; } = Id;

    [JsonIgnore]
    [Required]
    private string Password { get; set; } = Password;

    [Required]
    public Roles Role { get; set; } = Roles.CUSTOMER;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public bool ComparePassword(string PlainText)
    {
        return PasswordHasher.VerifyPassword(PlainText, Password);
    }

    public bool ChangePassword(string CurrentPassword, string NewPassword)
    {
        if (Password == PasswordHasher.HashPassword(CurrentPassword))
        {
            Password = PasswordHasher.HashPassword(NewPassword);
            return true;
        }

        return false;
    }
}