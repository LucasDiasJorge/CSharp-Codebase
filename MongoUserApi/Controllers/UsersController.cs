using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoUserApi.Models;
using MongoUserApi.Services;

namespace MongoUserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll(CancellationToken cancellationToken)
    {
        List<User> users = await _service.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(string id, CancellationToken cancellationToken)
    {
        User? user = await _service.GetAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> Register(UserCreateRequest request, CancellationToken cancellationToken)
    {
        User created = await _service.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(UserLoginRequest request, CancellationToken cancellationToken)
    {
        AuthResponse? token = await _service.LoginAsync(request, cancellationToken);
        if (token == null)
        {
            return Unauthorized();
        }
        return Ok(token);
    }

    [Authorize(Policy = "ActiveUser")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, UserUpdateRequest request, CancellationToken cancellationToken)
    {
        bool updated = await _service.UpdateAsync(id, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        bool deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
