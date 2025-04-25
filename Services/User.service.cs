using API.Data;
using API.DTOs.User;
using API.Models;
using API.Utils;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Services;

public interface IUserService
{
    Task<ServiceResult<User>> CreateUserAsync(RegisterDto dto);
    Task<ServiceResult<User>> GetUserAsync(string Email);
    Task<ServiceResult<User>> GetUserByIdAsync(string Id);
}

public class UserService(AppDbContext _dbContext, IdGeneratorService _idGenerator) : IUserService
{
    private readonly AppDbContext _dbContext = _dbContext;
    private readonly IdGeneratorService _idGenerator = _idGenerator;

    public async Task<ServiceResult<User>> CreateUserAsync(RegisterDto dto)
    {
        User newUser = new(_idGenerator.GenerateId(), dto.FullName, dto.Email.ToLower(), PasswordHasher.HashPassword(dto.Password));
        try
        {
            await _dbContext.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<User>.Success("User Created successfully", newUser);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            {
                return ServiceResult<User>.Failure("Email already in use");
            }
            throw;
        }
    }

    public async Task<ServiceResult<User>> GetUserAsync(string Email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(p => p.Email == Email);
        if (user != null)
        {
            return ServiceResult<User>.Success("ok", user);
        }
        return ServiceResult<User>.Failure("user notfound");
    }

    public async Task<ServiceResult<User>> GetUserByIdAsync(string Id)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(p => p.Id.ToString() == Id);
        if (user != null)
        {
            return ServiceResult<User>.Success("ok", user);
        }
        return ServiceResult<User>.Failure("user notfound");
    }
}