using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdvancedAuthSystem.Data;
using AdvancedAuthSystem.Models;
using AdvancedAuthSystem.DTOs.Resource;
using AdvancedAuthSystem.Authorization.Policies;
using System.Security.Claims;

namespace AdvancedAuthSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public ResourceController(AppDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var resources = await _context.Resources
            .Include(r => r.Owner)
            .Where(r => r.IsPublic)
            .Select(r => new ResourceDto(
                r.Id,
                r.Name,
                r.Description,
                r.OwnerId,
                r.Owner.Username,
                r.Department,
                r.CreatedAt,
                r.IsPublic
            ))
            .ToListAsync();

        return Ok(resources);
    }

    [HttpGet("my-resources")]
    public async Task<IActionResult> GetMyResources()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var resources = await _context.Resources
            .Include(r => r.Owner)
            .Where(r => r.OwnerId == userId)
            .Select(r => new ResourceDto(
                r.Id,
                r.Name,
                r.Description,
                r.OwnerId,
                r.Owner.Username,
                r.Department,
                r.CreatedAt,
                r.IsPublic
            ))
            .ToListAsync();

        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var resource = await _context.Resources
            .Include(r => r.Owner)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resource == null)
            return NotFound();

        // Check authorization using resource-based policy
        var authResult = await _authorizationService.AuthorizeAsync(User, resource, PolicyNames.ResourceOwner);
        
        if (!authResult.Succeeded && !resource.IsPublic)
            return Forbid();

        var dto = new ResourceDto(
            resource.Id,
            resource.Name,
            resource.Description,
            resource.OwnerId,
            resource.Owner.Username,
            resource.Department,
            resource.CreatedAt,
            resource.IsPublic
        );

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.MinimumAge18)]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var resource = new Resource
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            Department = request.Department,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.UtcNow
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        var owner = await _context.Users.FindAsync(userId);
        var dto = new ResourceDto(
            resource.Id,
            resource.Name,
            resource.Description,
            resource.OwnerId,
            owner!.Username,
            resource.Department,
            resource.CreatedAt,
            resource.IsPublic
        );

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResourceRequest request)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
            return NotFound();

        // Resource-based authorization
        var authResult = await _authorizationService.AuthorizeAsync(User, resource, PolicyNames.ResourceOwner);
        if (!authResult.Succeeded)
            return Forbid();

        if (request.Name != null)
            resource.Name = request.Name;
        if (request.Description != null)
            resource.Description = request.Description;
        if (request.IsPublic.HasValue)
            resource.IsPublic = request.IsPublic.Value;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Resource updated successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
            return NotFound();

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Resource deleted successfully" });
    }

    [HttpGet("it-resources")]
    [Authorize(Policy = PolicyNames.ITDepartment)]
    public async Task<IActionResult> GetITResources()
    {
        var resources = await _context.Resources
            .Include(r => r.Owner)
            .Where(r => r.Department == "IT")
            .Select(r => new ResourceDto(
                r.Id,
                r.Name,
                r.Description,
                r.OwnerId,
                r.Owner.Username,
                r.Department,
                r.CreatedAt,
                r.IsPublic
            ))
            .ToListAsync();

        return Ok(resources);
    }

    [HttpPost("sensitive-operation")]
    [Authorize(Policy = PolicyNames.WorkingHours)]
    public IActionResult PerformSensitiveOperation()
    {
        return Ok(new { message = "Sensitive operation completed during working hours" });
    }

    [HttpGet("restricted-content")]
    [Authorize(Policy = PolicyNames.MinimumAge21)]
    public IActionResult GetRestrictedContent()
    {
        return Ok(new { message = "Access granted to restricted content (21+)" });
    }
}
