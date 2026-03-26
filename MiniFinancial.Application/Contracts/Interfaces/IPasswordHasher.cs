namespace MiniFinancial.Application.Contracts.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(Object obj, string password);

        bool Verify(Object obj, string password, string passwordHash);
    }
}
