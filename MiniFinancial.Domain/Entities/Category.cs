using MiniFinancial.Domain.Commom;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.Entities
{
    public class Category : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Name { get; private set; }

        public decimal? MonthlyLimit { get; private set; }

        protected Category() { }

        public Category(Guid userId, string name, decimal? monthlyLimit, DateTimeOffset now) : base(now)
        {
            if (userId == Guid.Empty) throw new BusinessRuleException("Usuário inválido.");
            if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Nome da categoria é obrigatório.");
            if (monthlyLimit.HasValue && monthlyLimit.Value <= 0) throw new BusinessRuleException("Limite mensal deve ser maior que zero.");

            UserId = userId;
            Name = name.Trim();
            MonthlyLimit = monthlyLimit;
        }

        public void SetMonthlyLimit(decimal? monthlyLimit, DateTimeOffset now)
        {
            if (monthlyLimit.HasValue && monthlyLimit.Value <= 0) throw new BusinessRuleException("Limite mensal deve ser maior que zero.");

            MonthlyLimit = monthlyLimit;
            MarkAsUpdated(now);
        }

        public void Rename(string name, DateTimeOffset now) 
        {  
            Name = name.Trim(); 
            MarkAsUpdated(now); 
        }

        public void SetMonthlyLimit(decimal limit, DateTimeOffset now)
        {
            MonthlyLimit = limit;
            MarkAsUpdated(now);
        }

        public void Delete(DateTimeOffset now)
        {
            if (IsDeleted) return;

            MarkAsDeleted(now);
        }
    }
}
