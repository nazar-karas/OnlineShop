using Application.DTO;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Configuration;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Presentation.Messages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, IConfiguration configuration, IUserService userService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
        }

        [Authorize(Roles = "admin")]
        [HttpPut("whitelist/add")]
        public async Task<IActionResult> AddUsersToWhiteList([FromQuery] string ids)
        {
            List<Guid> guids = ids.Split(",").Select(x => Guid.Parse(x)).ToList();
            OperationResponse response = await _userService.ChangeUsersWhiteListStatus(guids, true);
            return Ok(response);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("whitelist/remove")]
        public async Task<IActionResult> RemoveUsersFromWhiteList([FromQuery] string ids)
        {
            List<Guid> guids = ids.Split(",").Select(x => Guid.Parse(x)).ToList();
            OperationResponse response = await _userService.ChangeUsersWhiteListStatus(guids, false);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto request)
        {
            try
            {
                var user = await _userService.Create(request);

                return Ok(user);
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken([FromBody] TokenGenerationRequest request)
        {
            JwtSettingsConfiguration configuration = new JwtSettingsConfiguration();
            _configuration.GetSection("Security:JwtSettings").Bind(configuration);

            byte[] secret = Encoding.UTF8.GetBytes(configuration.Key);

            var user = await _userService.Get(request.Email);
            
            if (user == null)
            {
                return NotFound($"The user with email '{request.Email}' not found");
            }

            bool passwordIsValid = await _userService.CheckPassword(user.Id, request.Password);

            if (!passwordIsValid)
            {
                return BadRequest(new { Error = "The provided user password is invalid." });
            }

            if (!user.IsInWhiteList)
            {
                return BadRequest(new { Error = "The user is not in white list." });
            }

            string role = user?.Role;

            var claims = new List<Claim>()
            {
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("role", role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(configuration.TokenLifetime),
                Issuer = configuration.Issuer,
                Audience = configuration.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwt = tokenHandler.WriteToken(token);

            return Ok(jwt);
        }

        [Authorize]
        [HttpPost("token/refresh")]
        public IActionResult RefreshToken()
        {
            return Ok();
        }

        [Authorize]
        [HttpGet("profiles/{id}")]
        public async Task<IActionResult> UserProfile(Guid id)
        {
            var user = await _userService.Get(id);
            return Ok(user);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("profiles")]
        public async Task<IActionResult> UsersProfiles([FromQuery]QueryParams query)
        {
            var users = await _userService.GetAll(query);
            return Ok(users);
        }

        [Authorize]
        [HttpPut("profiles")]
        public async Task<IActionResult> UpdateUserProfile(UserUpdateDto userDto)
        {
            try
            {
                var user = await _userService.Update(userDto);
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (AlreadyExistsException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }

        }

        [Authorize]
        [HttpPost("password/change/request/{userId}")]
        public async Task<IActionResult> AskForPasswordUpdate(Guid userId)
        {
            var code = await _userService.AskForPasswordUpdate(userId);
            return Ok(code);
        }

        [Authorize]
        [HttpPost("password/change")]
        public async Task<IActionResult> ChangeUserPassword(UserChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePassword(request.Email, request.NewPassword, request.ConfirmationCode);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUsers([FromQuery]string ids)
        {
            return Ok();
        }
    }
}
