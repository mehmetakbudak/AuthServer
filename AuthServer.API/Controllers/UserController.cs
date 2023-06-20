using AuthServer.Core.Dtos;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Exceptions;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto model)
        {
            var result = await _userService.CreateUserAsync(model);
            return ActionResultInstance(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var result = await _userService.GetUserByNameAsync(User.Identity.Name);
            return ActionResultInstance(result);
        }

        [Authorize]
        [HttpPost("CreateUserRoles/{userName}")]
        public async Task<IActionResult> CreateUserRoles(string userName)
        {
            var result = await _userService.CreateUserRoleAsync(userName);
            return ActionResultInstance(result);
        }
    }
}
