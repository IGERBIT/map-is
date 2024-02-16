using MapShared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("org")]
[ApiController]
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
        var org = new Organization
        {
            Name = dto.OrgName,
            Desc = dto.OrgArea,
            Site = dto.Contacts.Site,
            Phone = dto.Contacts.Phone,
            Address = dto.Contacts.Address
        };

        var owner = new Member
        {
            FullName = dto.Owner.Fullname,
            Email = dto.Owner.Email,
            Password = F.HashSHA256(dto.Owner.Password),
            AssignDate = DateTime.Now,
            Role = Role.Owner,
            Organization = org
        };
        
        org.Members.Add(owner);

        _mapContext.Organizations.Add(org);
        _mapContext.Members.Add(owner);

        await _mapContext.SaveChangesAsync();

        return "OK";
    }
    
    [Authorize]
    [HttpPost("add-member")]
    public async Task<ActionResult<Guid>> Create([FromBody]CreateMemberDto dto)
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();

        var owner = await _mapContext.Members.FindAsync(guid);
        if (owner is not { Role: Role.Owner }) return Unauthorized();
        
        var newMember = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = F.HashSHA256(dto.Password),
            AssignDate = DateTime.Now,
            OrganizationId = owner.OrganizationId,
            Role = Role.User
        };

        _mapContext.Members.Add(newMember);

        await _mapContext.SaveChangesAsync();

        return newMember.Id;
    }
    
    [Authorize]
    [HttpGet("members")]
    public async Task<ActionResult<IEnumerable<MemberDto>>> Create()
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();

        var owner = await _mapContext.Members.FindAsync(guid);
        if (owner is not { Role: Role.Owner }) return Unauthorized();

        return _mapContext.Members.Where(x => x.OrganizationId == owner.OrganizationId).Select(x => new MemberDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            AssignDate = x.AssignDate
        }).ToList();
    }
    
    [Authorize]
    [HttpPost("remove-member/{id:Guid}")]
    public async Task<IActionResult> Remove([FromQuery] Guid id)
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();

        var owner = await _mapContext.Members.FindAsync(guid);
        if (owner is not { Role: Role.Owner }) return Unauthorized();

        var memberToDelete = _mapContext.Members.FirstOrDefault(x => x.Id == id && x.OrganizationId == owner.OrganizationId);
        if (memberToDelete is null) return NotFound();

        _mapContext.Members.Remove(memberToDelete);

        await _mapContext.SaveChangesAsync();

        return Ok();
    }
    
    [Authorize]
    [HttpPost("update-member/{id:Guid}")]
    public async Task<IActionResult> Remove([FromQuery] Guid id, [FromBody] CreateMemberDto dto)
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();

        var owner = await _mapContext.Members.FindAsync(guid);
        if (owner is not { Role: Role.Owner }) return Unauthorized();

        var memberToEdit = _mapContext.Members.FirstOrDefault(x => x.Id == id && x.OrganizationId == owner.OrganizationId);
        if (memberToEdit is null) return NotFound();

        memberToEdit.FullName = dto.FullName;
        memberToEdit.Email = dto.Email;
        if (dto.Password is not null)
        {
            memberToEdit.Password = F.HashSHA256(dto.Password);
        }
        
        
        await _mapContext.SaveChangesAsync();

        return Ok();
    }
}

