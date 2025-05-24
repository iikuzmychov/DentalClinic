using DentalClinic.WebApi.Models.Requests;
using DentalClinic.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    public LoginResponse Login([FromBody] LoginRequest request)
    {
        throw new NotImplementedException();
    }
}
