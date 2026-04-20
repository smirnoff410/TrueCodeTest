using Common.Command;
using UserService.Application.Repository;
using UserService.Application.Request.User;
using UserService.Application.Services;

namespace UserService.Application.Command
{
    public class LoginCommand : ICommandHandler<LoginRequest, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public LoginCommand(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }
        public async Task<CommandResult<string>> Execute(LoginRequest command)
        {
            var user = await _userRepository.GetByNameAndPassword(command.Name, command.Password);
            if(user == null)
                return new CommandResult<string> { Success = false, ErrorMessage = $"User not found with name {command.Name}" };

            var token = _tokenService.GenerateAccessToken(user.Id);

            return new CommandResult<string> { Success = true, Response = token };
        }
    }
}
