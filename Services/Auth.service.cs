using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using API.DTOs.User;
using API.Models;
using API.Utils;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public interface IAuthService
{
    public Task<ServiceResult<User>> RegisterAsync(RegisterDto dto);
    public Task<ServiceResult<User>> LoginAsync(LoginDto dto);
    public void SetJwtTokens(string userId, string email, Roles role);
    public Task<bool> RefreshTokenAsync(string refresh_token);
}

public class AuthService(IUserService _userService, IConfiguration _configuration, IHttpContextAccessor _httpContextAccessor) : IAuthService
{
    private readonly IConfiguration _configuration = _configuration;
    private readonly IUserService _userService = _userService;
    private readonly IHttpContextAccessor _httpContextAccessor = _httpContextAccessor;

    public async Task<ServiceResult<User>> RegisterAsync(RegisterDto dto)
    {
        return await _userService.CreateUserAsync(dto);
    }

    public async Task<ServiceResult<User>> LoginAsync(LoginDto dto)
    {
        var result = await _userService.GetUserAsync(dto.Email.ToLower());

        if (result.IsSuccess)
        {
            var user = result.Data;
            if (user.ComparePassword(dto.Password))
            {
                SetJwtTokens(user.Id.ToString(), user.Email.ToLower(), user.Role);
                return ServiceResult<User>.Success("logined successfully", user);
            }
        }
        throw new HttpRequestException("invalid credentials", null, HttpStatusCode.BadRequest);
    }

    public void SetJwtTokens(string userId, string userEmail, Roles role)
    {
        var context = _httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("HttpContext is null. Cannot set cookies outside of a valid HTTP request context.");

        string access_token = GenerateAccessToken(userId, userEmail, role);
        string refresh_token = GenerateRefreshToken(userId);

        var cookieOptions_access = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(30).ToUniversalTime()
        };

        var cookieOptions_refresh = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(10).ToUniversalTime()
        };

        context.Response.Cookies.Append("refresh_token", refresh_token, cookieOptions_refresh);
        context.Response.Cookies.Append("access_token", access_token, cookieOptions_access);
    }

    public string GenerateAccessToken(string userId, string email, Roles role)
    {
        var claims = new List<Claim>{
            new (JwtRegisteredClaimNames.Sub, userId),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Email, email),
            new (ClaimTypes.Role, role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ??
            throw new InvalidOperationException("JWT:Key is not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_configuration["JWT:AccessTokenExpiryMinutes"] ?? "30")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(string userId)
    {
        var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ??
            throw new InvalidOperationException("JWT:Key is not configured")));


        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims,
            expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["JWT:RefreshTokenExpiryDays"] ?? "10")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> RefreshTokenAsync(string refresh_token)
    {
        var JwtHandler = new JwtSecurityTokenHandler();
        try
        {
            var token = await JwtHandler.ValidateTokenAsync(refresh_token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured")))
            });

            if (token.Exception != null)
            {
                return false;
            }

            string? userId = token.Claims.FirstOrDefault(claim => claim.Key == ClaimTypes.NameIdentifier).Value.ToString();

            if (userId != null)
            {
                var result = await _userService.GetUserByIdAsync(userId);
                if (result.IsSuccess)
                {
                    SetJwtTokens(result.Data.Id.ToString(), result.Data.Email, result.Data.Role);
                    return true;
                }
            }

            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }
}