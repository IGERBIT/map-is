using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MapShared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly MapContext _mapContext;
    
    public AuthController(MapContext mapContext)
    {
        _mapContext = mapContext;
    }

    [HttpPost("signin")]
    public async Task<Result<TokenDto, ApiError>> SignIn([FromBody]SignInDto signInDto)
    {
        var member = await GetMember(signInDto.Email, signInDto.Password);
        if (member is null)  return ApiError.Undefined("There are no such user");

        var identity = GetIdentity(member);
        
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(AuthOptions.LifetimeMin),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new TokenDto { Token = encodedJwt, IsOwner = member.Role == Role.Owner};
    }
    
    
    private async Task<Member?> GetMember(string email, string password)
    {
        var hash = F.HashSHA256(password);
        return await _mapContext.Members.FirstOrDefaultAsync(x=> x.Email == email && x.Password == hash);
    }
    
    private ClaimsIdentity GetIdentity(Member member)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, member.Email),
            new Claim("guid", member.Id.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, null);
        return claimsIdentity;
    }
    
}

