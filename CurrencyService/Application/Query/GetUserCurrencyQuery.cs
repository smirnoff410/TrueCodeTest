using Common.Command;
using CurrencyService.Application.Repository;
using CurrencyService.Application.Request;
using CurrencyService.Domain.Models;

namespace CurrencyService.Application.Query
{
    public class GetUserCurrencyQuery : ICommandHandler<GetUserCurrencyRequest, List<Currency>>
    {
        private readonly IUserCurrencyRepository _userCurrencyRepository;

        public GetUserCurrencyQuery(IUserCurrencyRepository userCurrencyRepository)
        {
            _userCurrencyRepository = userCurrencyRepository;
        }
        public async Task<CommandResult<List<Currency>>> Execute(GetUserCurrencyRequest command)
        {
            var result = await _userCurrencyRepository.GetByUserId(command.UserId);

            return new CommandResult<List<Currency>> { Success = true, Response = result };
        }
    }
}
