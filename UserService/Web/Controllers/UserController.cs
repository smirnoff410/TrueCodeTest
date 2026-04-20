using Common.Command;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Request.User;

namespace UserService.Web.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ICommandHandler<RegisterUserRequest, string> _registerCommand;
        private readonly ICommandHandler<LoginRequest, string> _loginCommand;

        public UserController(
            ICommandHandler<RegisterUserRequest, string> registerCommand,
            ICommandHandler<LoginRequest, string> loginCommand)
        {
            _registerCommand = registerCommand;
            _loginCommand = loginCommand;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest dto)
        {
            var result = await _registerCommand.Execute(dto);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            var result = await _loginCommand.Execute(dto);
            return Ok(result);
        }
    }
}
