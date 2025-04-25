using System.Security.Claims;
using API.DTOs.User;
using API.Models;
using API.Services;
using API.Utils;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService _authService, IUserService _userService) : ControllerBase
{
    public readonly IAuthService _authService = _authService;
    public readonly IUserService _userService = _userService;


    [HttpPost("register")]
    [ProducesResponseType(typeof(APIResult<User>), 200)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }
        var user = result.Data;
        _authService.SetJwtTokens(user.Id.ToString(), user.Email, user.Role);

        return Ok(new
        {
            message = result.Message,
            data = result.Data
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(APIResult<User>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new
            {
                message = result.Message,
                data = result.Data
            });
        }
        catch (Exception ex) when (ex.Message == "invalid credentials")
        {
            return Unauthorized(new { ex.Message });
        }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new
        {
            message = "logout successful"
        });
    }


    [HttpGet("user")]
    [EndpointSummary("Get user information")]
    public async Task<IActionResult> GetUser()
    {
        string? userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail))
        {
            var result = await _userService.GetUserAsync(userEmail);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    message = "ok",
                    data = result.Data
                });
            }
        }
        return Unauthorized();
    }

    [HttpGet("refresh")]
    [EndpointSummary("Refresh tokens")]
    public async Task<IActionResult> RefreshToken()
    {
        bool result = false;
        string? refresh_token = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refresh_token))
        {
            result = await _authService.RefreshTokenAsync(refresh_token);
        }
        return result ? Ok(new
        {
            message = "token refreshed"
        }) : Unauthorized(string.Empty);
    }
}