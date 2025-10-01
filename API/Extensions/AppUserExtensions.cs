using System;
using API.DTOs;
using API.Enities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
    public static UserDto toDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }
}