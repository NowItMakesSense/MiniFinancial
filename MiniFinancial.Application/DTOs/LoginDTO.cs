namespace MiniFinancial.Application.DTOs
{
    public record LoginDTO(string AccessToken, UserDTO? user, DateTimeOffset CreatedAt);
}
