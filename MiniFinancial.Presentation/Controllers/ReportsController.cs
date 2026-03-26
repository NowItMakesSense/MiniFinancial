using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Features.Reports.BalanceReport.Commads;
using MiniFinancial.Application.Features.Reports.CategoryReport.Commads;
using MiniFinancial.Application.Features.Reports.MonthlyReport.Commads;

namespace MiniFinancial.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public ReportsController(IMediator mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [Authorize]
        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] int year, [FromQuery] int month)
        {
            var userId = _currentUser.UserId;
            var result = await _mediator.Send(new GetMonthlyReportCommand(userId, year, month));

            return Ok(result);
        }

        [Authorize]
        [HttpGet("category-expenses")]
        public async Task<IActionResult> GetCategoryExpenses([FromQuery] int year, [FromQuery] int month)
        {
            var userId = _currentUser.UserId;
            var result = await _mediator.Send(new GetCategoryReportCommand(userId, year, month));

            return Ok(result);
        }

        [Authorize]
        [HttpGet("balance-evolution")]
        public async Task<IActionResult> GetBalanceEvolution([FromQuery] int year)
        {
            var userId = _currentUser.UserId;
            var result = await _mediator.Send(new GetBalanceReportCommand(userId, year));

            return Ok(result);
        }
    }
}
