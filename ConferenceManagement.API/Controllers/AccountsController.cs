using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using ConferenceManagement.Infrastructure.Data;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AccountsController(
    AccountRepository accounts,
    PersonRepository persons,
    IPasswordHasher<ConferenceAccount> hasher,
    IAntiforgery antiforgery,
    IWebHostEnvironment environment) : ControllerBase
{
    private const string OrganizerLogin = "organizer";

    [HttpGet("session")]
    public async Task<IActionResult> Session()
    {
        Guid? personId = null;
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            personId = (await accounts.GetAsync(id))?.PersonId;
        }

        var canSetUp = User.Identity?.IsAuthenticated != true &&
            IsLocalDevelopment() && !await accounts.HasOrganizerAsync();

        return Ok(new
        {
            login = User.Identity?.Name,
            isOrganizer = User.IsInRole(AccessRoles.Organizer),
            personId,
            canSetUp,
            csrfToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken
        });
    }

    [HttpPost("setup")]
    [EnableRateLimiting("SignIn")]
    public async Task<IActionResult> Setup(PasswordInput input)
    {
        if (!IsLocalDevelopment() || await accounts.HasOrganizerAsync())
        {
            return Forbid();
        }

        var account = new ConferenceAccount { Login = OrganizerLogin, IsOrganizer = true };
        account.PasswordHash = hasher.HashPassword(account, input.Password);
        await accounts.AddAsync(account);
        await SignInAsync(account);
        return NoContent();
    }

    [HttpPost("login")]
    [EnableRateLimiting("SignIn")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        var account = await accounts.FindAsync(input.Login.ToLowerInvariant());
        if (account is null)
        {
            return Unauthorized("Неверный логин или пароль.");
        }

        var result = hasher.VerifyHashedPassword(account, account.PasswordHash, input.Password);
        if (result is PasswordVerificationResult.Failed)
        {
            return Unauthorized("Неверный логин или пароль.");
        }

        if (result is PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.PasswordHash = hasher.HashPassword(account, input.Password);
            await accounts.SaveAsync();
        }

        await SignInAsync(account);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpPost("responsible")]
    [Authorize(Roles = AccessRoles.Organizer)]
    public async Task<IActionResult> CreateResponsible(ResponsibleInput input)
    {
        if (string.Equals(input.Login, OrganizerLogin, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Этот логин зарезервирован для организатора.");
        }

        if (await persons.GetByIdAsync(input.PersonId) is null)
        {
            return BadRequest("Выберите существующее ответственное лицо.");
        }

        var account = new ConferenceAccount { Login = input.Login.ToLowerInvariant(), PersonId = input.PersonId };
        account.PasswordHash = hasher.HashPassword(account, input.Password);
        await accounts.AddAsync(account);
        return NoContent();
    }

    private bool IsLocalDevelopment()
    {
        var address = HttpContext.Connection.RemoteIpAddress;
        var host = Request.Host.Host;
        return environment.IsDevelopment() && Request.IsHttps &&
            address is not null && IPAddress.IsLoopback(address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address) &&
            (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host is "127.0.0.1" or "[::1]");
    }

    private Task SignInAsync(ConferenceAccount account)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Login),
            new(ClaimTypes.Role, account.IsOrganizer ? AccessRoles.Organizer : AccessRoles.Responsible)
        ];
        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        return HttpContext.SignInAsync(scheme, new ClaimsPrincipal(new ClaimsIdentity(claims, scheme)));
    }

    public sealed record PasswordInput([Required, StringLength(128, MinimumLength = 8)] string Password);

    public sealed record LoginInput(
        [Required, StringLength(64)] string Login,
        [Required, StringLength(128)] string Password);

    public sealed record ResponsibleInput(
        [Required, RegularExpression("^[A-Za-z0-9_.-]{3,64}$")] string Login,
        [Required, StringLength(128, MinimumLength = 8)] string Password,
        Guid PersonId);
}
