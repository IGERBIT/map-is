using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MapShared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Controllers;

[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly MapContext _mapContext;
    
    public AuthController(MapContext mapContext)
    {
        _mapContext = mapContext;
    }

    [HttpPost("signin")]
    public async Task<Result<string, ApiError>> SignIn([FromBody]SignInDto signInDto)
    {
        var identity = await GetIdentity(signInDto.Email, signInDto.Password);

        if (identity is null)  return ApiError.Undefined("There are no such user");
        
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(AuthOptions.LifetimeMin),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return encodedJwt;
    }
    
    
    [HttpGet("whoiam")]
    public IActionResult SignIn()
    {
        if (User.Identity is { IsAuthenticated: true } identity)
        {
            return Ok($"guid: {identity.Name}");
        }

        return Ok("None");
    }
    
    
    private async Task<ClaimsIdentity?> GetIdentity(string email, string password)
    {
        var user = await _mapContext.Members.FirstOrDefaultAsync(x=> x.Email == email);
        if (user == null) return null;
        
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
            new Claim("guid", user.Id.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, null);
        return claimsIdentity;

    }
    
}

