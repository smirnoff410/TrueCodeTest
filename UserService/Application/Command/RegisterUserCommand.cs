using UserService.Application.Repository;
using UserService.Application.Request.User;
using Mapster;

namespace UserService.Application.Command
{
    using Common.Command;
    using Common.Repository;
    using UserService.Application.Services;
    using UserService.Domain.Models;
    public class RegisterUserCommand : ICommandHandler<RegisterUserRequest, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public RegisterUserCommand(IUnitOfWork unitOfWork, IUserRepository userRepository, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }
        public async Task<CommandResult<string>> Execute(RegisterUserRequest command)
        {
            var userExist = await _userRepository.IsExist(command.Name);
            if(userExist)
                return new CommandResult<string> { Success = false, ErrorMessage = $"User with name {command.Name} is exist" };

            var userId = await _userRepository.Add(command.Adapt<User>());

            var token = _tokenService.GenerateAccessToken(userId);

            await _unitOfWork.Save();

            return new CommandResult<string> { Success = true, Response = token };
        }
    }
}
