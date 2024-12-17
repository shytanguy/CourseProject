using Back.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Client>> RegisterUser(Client newUser)
    {
        var user = await _userService.RegisterUser(newUser);
        return CreatedAtAction(nameof(GetUser), new { userId = user.UserId }, user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Client>> Login(Client loginUser)
    {
        try
        {
            var user = await _userService.Login(loginUser.Username, loginUser.Password);
            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    [HttpPost("{userId}/shoppingcart")]
    public async Task<IActionResult> AddToShoppingCart(int userId, [FromBody] int productId)
    {
        await _userService.AddToShoppingCart(userId, productId);
        return NoContent();
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<Client>> GetUser(int userId)
    {
        try
        {
            return Ok(await _userService.GetUserById(userId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
