using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MapShared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Controllers;

[Route("org")]
public class OrganizationController : ControllerBase
{
    private readonly MapContext _mapContext;
    
    public OrganizationController(MapContext mapContext)
    {
        _mapContext = mapContext;
    }

    [HttpPost("create")]
    public async Task<Result<string, ApiError>> Create([FromBody]CreateOrganizationDto dto)
    {

        return "";
    }
    
}

