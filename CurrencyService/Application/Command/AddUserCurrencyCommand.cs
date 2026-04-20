using Common.Command;
using Common.Repository;
using CurrencyService.Application.Repository;
using CurrencyService.Application.Request;
using CurrencyService.Domain.Models;
using Mapster;

namespace CurrencyService.Application.Command
{
    public class AddUserCurrencyCommand : ICommandHandler<AddUserCurrencyRequest, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserCurrencyRepository _userCurrencyRepository;

        public AddUserCurrencyCommand(IUnitOfWork unitOfWork, IUserCurrencyRepository userCurrencyRepository)
        {
            _unitOfWork = unitOfWork;
            _userCurrencyRepository = userCurrencyRepository;
        }
        public async Task<CommandResult<Guid>> Execute(AddUserCurrencyRequest command)
        {
            var userCurrencyId = await _userCurrencyRepository.Add(command.Adapt<UserCurrency>());

            await _unitOfWork.Save();

            return new CommandResult<Guid> { Success = true, Response = userCurrencyId };
        }
    }
}
