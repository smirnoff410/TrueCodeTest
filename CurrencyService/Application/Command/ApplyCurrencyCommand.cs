using Common.Command;
using Common.Repository;
using CurrencyService.Application.Repository;
using CurrencyService.Application.Request;
using CurrencyService.Domain.Models;
using Mapster;

namespace CurrencyService.Application.Command
{
    public class ApplyCurrencyCommand : ICommandHandler<List<ApplyCurrencyRequest>, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyRepository _currencyRepository;

        public ApplyCurrencyCommand(IUnitOfWork unitOfWork, ICurrencyRepository currencyRepository)
        {
            _unitOfWork = unitOfWork;
            _currencyRepository = currencyRepository;
        }
        public async Task<CommandResult<bool>> Execute(List<ApplyCurrencyRequest> command)
        {
            var currencyIds = command.Select(x => x.Id);

            var updateCurrencies = await _currencyRepository.Get(currencyIds);

            foreach (var currency in updateCurrencies)
            {
                var commandCurrency = command.First(x => x.Id == currency.Id);
                currency.Name = commandCurrency.Name;
                currency.Rate = commandCurrency.Rate;
            }

            var createCurrencyIds = currencyIds.Except(updateCurrencies.Select(x => x.Id));
            foreach(var currencyId in createCurrencyIds)
            {
                var commandCurrency = command.First(x => x.Id == currencyId);
                await _currencyRepository.Add(commandCurrency.Adapt<Currency>());
            }

            await _unitOfWork.Save();

            return new CommandResult<bool> { Success = true };
        }
    }
}
