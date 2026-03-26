using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.DTOs
{
    public record UserDTO(Guid Id, string Name, string Email);
}
