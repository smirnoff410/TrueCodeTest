using Common.Command;
using CurrencyService.Application.Request;
using CurrencyService.Application.Services;
using CurrencyService.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICommandHandler<GetUserCurrencyRequest, List<Currency>> _getUserCurrencyCommand;
        private readonly ICommandHandler<AddUserCurrencyRequest, Guid> _addUserCurrencyCommand;

        public CurrencyController(
            ICurrentUser currentUser,
            ICommandHandler<GetUserCurrencyRequest, List<Currency>> getUserCurrencyCommand,
            ICommandHandler<AddUserCurrencyRequest, Guid> addUserCurrencyCommand)
        {
            _currentUser = currentUser;
            _getUserCurrencyCommand = getUserCurrencyCommand;
            _addUserCurrencyCommand = addUserCurrencyCommand;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserFavorites()
        {
            var currentUserId = _currentUser.GetUserId();
            if (currentUserId == null)
                return BadRequest("Not found user id");

            var result = await _getUserCurrencyCommand.Execute(new GetUserCurrencyRequest { UserId = currentUserId.Value });
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddUserFavorite([FromBody] string currencyId)
        {
            var currentUserId = _currentUser.GetUserId();
            if (currentUserId == null)
                return BadRequest("Not found user id");

            var result = await _addUserCurrencyCommand.Execute(new AddUserCurrencyRequest { UserId = currentUserId.Value, CurrencyId = currencyId });
            return Ok(result);
        }
    }
}
