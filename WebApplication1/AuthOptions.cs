using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1;

public static class AuthOptions
{
    public const string ISSUER = "MapServer";
    public const string AUDIENCE = "MapClient";
    public const string KEY = "shaman_cool_shaman_cool_shaman_cool_shaman_cool_";
    public static  TimeSpan LifetimeMin = TimeSpan.FromMinutes(10);
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.ASCII.GetBytes(KEY));
}

