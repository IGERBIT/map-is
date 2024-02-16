﻿using System.Security.Claims;
using MapShared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("schemas")]
public class SchemasController : ControllerBase
{
    private readonly MapContext _mapContext;
    private readonly IWebHostEnvironment _appEnvironment;

    public SchemasController(MapContext mapContext, IWebHostEnvironment appEnvironment)
    {
        _mapContext = mapContext;
        _appEnvironment = appEnvironment;
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<SchemaLiteDto>>> List()
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();
        
        var member = await _mapContext.Members.Include(x => x.Organization).ThenInclude(x => x.Schemas).FirstOrDefaultAsync(x => x.Id == guid);
        if (member is null) return Unauthorized();
        return Ok(member.Organization.Schemas.Select(x => new SchemaLiteDto()
        {
            Id = x.Id,
            Name = x.Name
        }));
    }
    
    
    
    [Authorize]
    [HttpGet("get/{id:int}")]
    public async Task<ActionResult<SchemaDto>> Get(int id)
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();

        var member = await _mapContext.Members.FindAsync(guid);
        if (member is null) return Unauthorized();
        
        var schema = await _mapContext.Schemas
            .Include(x => x.Nodes)
            .Include(x => x.Objects)
            .Include(x => x.Links)
            .Include(x => x.ObjectLinks)
            .FirstOrDefaultAsync(x=>x.Id == id && member.OrganizationId == x.OrganizationId);

        if (schema is null) return NotFound();

        var nodes = schema.Nodes.Select(x => new NodeDto(x.Id, new Vector2Dto(x.X, x.Y), x.SchemaId)).ToList();
        var objects = schema.Objects.Select(x => new MapObjectDto(x.Id, x.Name, new Vector2Dto(x.X, x.Y), new SizeDto(x.Width, x.Height), x.SchemaId)).ToList();
        var links = schema.Links.Select(x => new LinkDto(x.Id, x.NodeAId, x.NodeBId, x.SchemaId)).ToList();
        var objLinks = schema.ObjectLinks.Select(x => new ObjectLinkDto(x.Id, x.NodeId, x.ObjectId, x.SchemaId)).ToList();

        return new SchemaDto()
        {
            Id = schema.Id,
            Name = schema.Name,
            ImageUrl = schema.ImagePath,
            
            Nodes = nodes,
            Objects = objects,
            Links = links,
            ObjectLinks = objLinks
        };
    }
    
    [Authorize]
    [HttpPost("create")]
    [RequestFormLimits(BufferBody = true)]
    public async Task<IActionResult> Create([FromForm] string schemeName, [FromForm] IFormFile file)
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();
        
        if (string.IsNullOrWhiteSpace(schemeName)) return BadRequest();

        var member = await _mapContext.Members.FindAsync(guid);
        if (member is null) return Unauthorized();
        
        var path = $"image_map_{Guid.NewGuid()}.png";

        using (var fileStream = new FileStream(Path.Combine(_appEnvironment.WebRootPath, path), FileMode.OpenOrCreate))
        {
            await file.CopyToAsync(fileStream);
            
        };

        _mapContext.Schemas.Add(new Schema()
        {
            Name = schemeName,
            ImagePath = path,
            OrganizationId = member.OrganizationId
        });

        await _mapContext.SaveChangesAsync();

        return Ok();
    }
    
    [Authorize]
    [HttpGet("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!F.TryAuth(User.Identity, out var guid)) return Unauthorized();

        var member = await _mapContext.Members.FindAsync(guid);
        if (member is null) return Unauthorized();
        
        if (member.Role != Role.Owner) return BadRequest("You must be an owner to delete schemas");

        var schema = await _mapContext.Schemas.FirstOrDefaultAsync(x => x.Id == id && x.OrganizationId == member.OrganizationId);
        if (schema is null) return NotFound();

        _mapContext.Schemas.Remove(schema);

        await _mapContext.SaveChangesAsync();

        return Ok();
    }
    
    [Authorize]
    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody]SchemaDto schemaToSave)
    {
        var schema = await _mapContext.Schemas
            .Include(x => x.Nodes)
            .Include(x => x.Objects)
            .Include(x => x.Links)
            .Include(x => x.ObjectLinks)
            .FirstOrDefaultAsync(x=>x.Id == schemaToSave.Id);

        if (schema is null) return NotFound();

        if (schemaToSave.ImageUrl is { } url)
        {
            schema.ImagePath = url;
        }
        
        if (schemaToSave.Name is { } name)
        {
            schema.Name = name;
        }
        
        var oLinksToDelete = schema.ObjectLinks.ExceptBy(schemaToSave.ObjectLinks.Select(x => x.Id), x => x.Id);
        var linksToDelete = schema.Links.ExceptBy(schemaToSave.Links.Select(x => x.Id), x => x.Id);
        var nodesToDelete = schema.Nodes.ExceptBy(schemaToSave.Nodes.Select(x => x.Id), x => x.Id);
        var objectsToDelete = schema.Objects.ExceptBy(schemaToSave.Objects.Select(x => x.Id), x => x.Id);
        
        _mapContext.NodeObjectLinks.RemoveRange(oLinksToDelete);
        _mapContext.NodeLinks.RemoveRange(linksToDelete);
        _mapContext.Nodes.RemoveRange(nodesToDelete);
        _mapContext.Objects.RemoveRange(objectsToDelete);
        
        var nodesToAdd = schemaToSave.Nodes.ExceptBy(schema.Nodes.Select(x => x.Id), x => x.Id).Select(x => new MapNode()
        {
            Id = x.Id,
            X = x.Position.X,
            Y = x.Position.Y,
            SchemaId = schema.Id,
            Schema = schema
        }).ToList();
        var objectsToAdd = schemaToSave.Objects.ExceptBy(schema.Objects.Select(x => x.Id), x => x.Id).Select(x => new ObjectInstance()
        {
            Id = x.Id,
            Name = x.Name,
            X = x.Position.X,
            Y = x.Position.Y,
            Width = x.Size.Width,
            Height = x.Size.Height,
            SchemaId = schema.Id,
            Schema = schema
        }).ToList();

        var allNodes = schema.Nodes.Concat(nodesToAdd);
        var allObjects = schema.Objects.Concat(objectsToAdd);
        
        var oLinksToAdd = schemaToSave.ObjectLinks.ExceptBy(schema.ObjectLinks.Select(x => x.Id), x => x.Id).Select(x=>  new NodeObjectLink()
        {
            Schema = schema,
            Node = allNodes.First(n=> n.Id == x.NodeId),
            Object = allObjects.First(n=> n.Id == x.ObjectId),
        }).ToList();
        // ReSharper disable PossibleMultipleEnumeration
        var linksToAdd = schemaToSave.Links.ExceptBy(schema.Links.Select(x => x.Id), x => x.Id).Select( x=> new NodeLink()
        {
            Schema = schema,
            NodeA = allNodes.First(n=> n.Id == x.NodeAId),
            NodeB = allNodes.First(n=> n.Id == x.NodeBId),
        }).ToList();
        // ReSharper restore PossibleMultipleEnumeration

        _mapContext.Nodes.AddRange(nodesToAdd);
        _mapContext.Objects.AddRange(objectsToAdd);
        _mapContext.NodeObjectLinks.AddRange(oLinksToAdd);
        _mapContext.NodeLinks.AddRange(linksToAdd);
        
        await _mapContext.SaveChangesAsync();
        
        return Ok();
    }
}

