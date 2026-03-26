using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniFinancial.Application.Features.Categories.Commands;

namespace MiniFinancial.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetCategoryByIdCommand(id);

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess) return NotFound(result.Errors);

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] RegisterCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("Id da rota diferente do corpo.");

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id, [FromBody] RemoveCategoryCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("Id da rota diferente do corpo.");

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return NoContent();
        }
    }
}
