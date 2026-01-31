using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Models.DTOs;
using SafeVault.Services;

namespace SafeVault.Controllers;

[ApiController]
[Authorize] // Require authentication for all endpoints
[Route("api/[controller]")]
public class SecretController : ControllerBase
{
    private readonly ISecretService _secretService;

    public SecretController(ISecretService secretService)
    {
        _secretService = secretService;
    }

    /// <summary>
    /// Gets all secrets for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllSecrets()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var secrets = await _secretService.GetAllSecretsByUserIdAsync(userId.Value);
        return Ok(secrets);
    }

    /// <summary>
    /// Gets a specific secret by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSecretById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var secret = await _secretService.GetSecretByIdAsync(id, userId.Value);
        
        if (secret == null)
        {
            return NotFound(new { message = "Secret not found" });
        }
        
        return Ok(secret);
    }

    /// <summary>
    /// Creates a new secret
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSecret([FromBody] SecretCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var (success, secretId) = await _secretService.CreateSecretAsync(createDto, userId.Value);
        
        if (!success || secretId == null)
        {
            return BadRequest(new { message = "Failed to create secret" });
        }
        
        return CreatedAtAction(nameof(GetSecretById), new { id = secretId }, null);
    }

    /// <summary>
    /// Updates an existing secret
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSecret(int id, [FromBody] SecretUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var success = await _secretService.UpdateSecretAsync(id, updateDto, userId.Value);
        
        if (!success)
        {
            return NotFound(new { message = "Secret not found or you don't have permission to update it" });
        }
        
        return Ok(new { message = "Secret updated successfully" });
    }

    /// <summary>
    /// Deletes an existing secret
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSecret(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated properly" });
        }
        
        var success = await _secretService.DeleteSecretAsync(id, userId.Value);
        
        if (!success)
        {
            return NotFound(new { message = "Secret not found or you don't have permission to delete it" });
        }
        
        return NoContent();
    }

    /// <summary>
    /// Helper method to extract the current user ID from claims
    /// </summary>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        
        return userId;
    }
}
