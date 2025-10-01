using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Enities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
    {
        if (await EmailExists(registerDTO.Email))
        {
            return BadRequest("Email is already in use.");
        }
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Email = registerDTO.Email,
            UserName = registerDTO.DisplayName,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.toDto(tokenService);
    }

    [HttpPost("login")] // POST: api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDTO)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == loginDTO.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid email or password.");
            }
        }

        return user.toDto(tokenService);
    }

    private async Task<bool> EmailExists(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email.ToLower());
    }
}
