namespace MiniFinancial.Domain.Exceptions
{
    public class OwnershipViolationException : Exception
    {
        public OwnershipViolationException(string message) : base(message)
        {
        }
    }
}
