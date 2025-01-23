using Annapurnaworld.common;
using Annapurnaworld.entity;
using Annapurnaworld.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Annapurnaworld.entity.LoginRequest;

namespace Annapurnaworld.api.Controllers
{
    [AllowAnonymous]
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AccountController(ITokenService tokenService, IUserService userService)
        {
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.GetUserByEmail(request.Email);
            if (user is not null && PasswordHelper.VerifyPassword(request.Password,user.Password))
            {
                var userId = user.Id;
                var token = _tokenService.GenerateJwtToken(userId.ToString());
                var refreshToken = _tokenService.GenerateRefreshToken(userId.ToString());
                return Ok(new
                {
                    IsAuthenticate = true,
                    UserName = user.FullName,
                    UserRoles = user.UserRoles.Select(x => x.Role.Name),
                    Token = token,
                    RefreshToken = refreshToken
                });
            }
            return Unauthorized();
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userService.GetUserByEmail(request.Email);
            if (user is null)
            {
                var userId = Guid.NewGuid();
                User userObj = new User { Id = userId, FullName = request.Name, Email = request.Email, IsActive = true, Password = PasswordHelper.HashPassword(request.Password) };
                var role = await _userService.GetRoleByName(request.Role);
                if (role == null)
                {
                    return BadRequest();
                }
                var useerRole = new UserRole() { RoleId = role.Id, UserId = userId };
                userObj.UserRoles = new List<UserRole>();
                userObj.UserRoles.Add(useerRole);
                await _userService.AddUser(userObj);
                return Ok(true);
            }
            return Ok(user);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userService.GetUserByEmail(request.Email);
            if (user is not null)
            {
                user.Password = PasswordHelper.HashPassword(request.Password);
                await _userService.UpdateUser(user);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users.Select(x=>x.Email));
        }
    }
}
