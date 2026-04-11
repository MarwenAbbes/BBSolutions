using Asp.Versioning;
using BB.Infrastructure.Models;
using BB.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BB.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll() =>
        Ok(await _userService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        var created = await _userService.CreateAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> Update(int id, [FromBody] User updated)
    {
        var user = await _userService.UpdateAsync(id, updated);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}