using CustomerSalesSystem.API.Services;
using CustomerSalesSystem.Application.Features.User.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomerSalesSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]  RegisterUserCommand request)
        {
            
            if (request == null) {
                return BadRequest("Invalid registration request");
            }
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username and password are required");
            }
            try
            {
                var isRegistered = await _mediator.Send(request);

                if (!isRegistered)
                    return BadRequest("User registration failed.");

                return Ok("User registered successfully");
            }
            catch (ArgumentException ex)
            {
                // Expected errors like invalid input
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Unexpected errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand request)
        {
            if (request == null)
                return BadRequest("Invalid login request.");

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and password are required.");

            try
            {
                var loginResponse = await _mediator.Send(request);

                if (loginResponse == null)
                    return Unauthorized("Invalid username or password.");

                // Return access and refresh tokens
                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                // Unexpected server errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required.");

            try
            {
                var loginResponse = await _mediator.Send(request);

                if (loginResponse == null)
                    return Unauthorized("Invalid or expired refresh token.");

                // Return new access and refresh tokens
                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                // Unexpected server errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            // Extract user ID from claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token.");

            // You can also get the username from claims
            var username = User.Identity?.Name ?? "Unknown";

            return Ok(new
            {
                UserId = userIdClaim,
                Username = username
            });
        }


    }
}
