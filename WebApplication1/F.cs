using System.Security.Claims;
using System.Security.Principal;

namespace WebApplication1;

public static class F
{
    public static bool TryAuth(IIdentity? identity, out Guid guid)
    {
        guid = Guid.Empty;
        return identity is ClaimsIdentity claimsIdentity && claimsIdentity.FindFirst("guid") is { } guidClaim && Guid.TryParse(guidClaim.Value, out guid);
    }
}

