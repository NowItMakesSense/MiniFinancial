using MiniFinancial.Domain.Commom;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.Entities
{
    public class Account : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Name { get; private set; }

        public decimal Balance { get; private set; }

        public bool AllowNegativeBalance { get; private set; }

        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions;

        protected Account() { }

        public Account(Guid userId, string name, bool allowNegativeBalance, DateTimeOffset now) : base(now)
        {
            if (userId == Guid.Empty) throw new BusinessRuleException("Usuário inválido.");

            if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Nome da conta é obrigatório.");

            UserId = userId;
            Name = name.Trim();
            AllowNegativeBalance = allowNegativeBalance;
            Balance = 0m;
        }

        public void Rename(string name, DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Nome inválido.");

            Name = name.Trim();
            MarkAsUpdated(now);
        }

        public void SetBalance(decimal balance, DateTimeOffset now)
        {
            if (AllowNegativeBalance && Balance < 0) throw new BusinessRuleException("A conta nao permite valores negativos.");

            Balance = balance;
            MarkAsUpdated(now);
        }

        public void Delete(DateTimeOffset now)
        {
            if (IsDeleted) return;

            MarkAsDeleted(now);
        }
    }
}
