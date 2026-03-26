using MiniFinancial.Domain.Commom;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.Entities
{
    public class IdempotencyRequest : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Key { get; private set; }

        public string RequestHash { get; private set; }

        public string? ResponseJson { get; private set; }

        protected IdempotencyRequest() { }

        public IdempotencyRequest(Guid userId, string key, string requestHash, DateTimeOffset now)
            : base(now)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new BusinessRuleException("Idempotency Key é obrigatória.");

            if (string.IsNullOrWhiteSpace(requestHash))
                throw new BusinessRuleException("Hash da requisição é obrigatório.");

            UserId = userId;
            Key = key;
            RequestHash = requestHash;
        }

        public void SetResponse(string responseJson, DateTimeOffset now)
        {
            ResponseJson = responseJson;
            MarkAsUpdated(now);
        }
        public void ValidateHash(string requestHash)
        {
            if (RequestHash != requestHash)
                throw new BusinessRuleException("Requisição com mesma chave mas payload diferente.");
        }
    }
}
