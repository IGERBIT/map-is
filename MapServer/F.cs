using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace WebApplication1;

public static class F
{
    public static bool TryAuth(IIdentity? identity, out Guid guid)
    {
        guid = Guid.Empty;
        return identity is ClaimsIdentity claimsIdentity && claimsIdentity.FindFirst("guid") is { } guidClaim && Guid.TryParse(guidClaim.Value, out guid);
    }


    public static string HashSHA256(string text)
    {
        var crypt = SHA256.Create();
        var hash = string.Empty;
        var crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(text));
        return crypto.Aggregate(hash, (current, theByte) => current + theByte.ToString("x2"));
    }
    
    
}

