using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Helper.Helper;
using Microsoft.Extensions.Logging;
using Dto.Auth;
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Login endpoint to authenticate users and return a JWT token
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {

        // Authenticate user from database
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || request.Password == null ||
            !AuthHelper.VerifyPasswordHash(request.Password, user.Password))
        {
            return Unauthorized("Invalid credentials");
        }

        // Generate JWT token
        var token = AuthHelper.GenerateToken(user);

        return Ok(new { Token = token });
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already exists", code = 200 });
        }
        if (request.Password == null || request.Password.Length < 6 || request.Username == null || request.Username.Length < 6)
        {
            return BadRequest(new { message = "Username and Password must be at least 6 characters long", code = 201 });
        }


        var hashedPassword = AuthHelper.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Password = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully");
    }

}
