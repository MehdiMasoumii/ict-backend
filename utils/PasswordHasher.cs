namespace API.Utils;

public class PasswordHasher
{
    public static string HashPassword(string plainPassword)
    {
        // Generate a salt and hash the password
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }
    public static bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        // Verify that the plain password matches the hashed password
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}