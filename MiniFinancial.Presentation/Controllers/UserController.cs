using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniFinancial.Application.Features.Users.Commands;

namespace MiniFinancial.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdCommand(id);

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess) return NotFound(result.Errors);

            return Ok(result.Value);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("Id da rota diferente do corpo.");

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var command = new RemoveUserCommand(id);

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return NoContent();
        }
    }
}
