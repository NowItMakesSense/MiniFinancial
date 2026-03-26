using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Commom;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public ICollection<UserRefreshToken> RefreshTokens { get; private set; } = new List<UserRefreshToken>();

        protected User() { }

        public User(string name, string email, string passwordHash, UserRole role, DateTimeOffset now) : base(now)
        {
            SetName(name);
            SetEmail(email);
            SetPassword(passwordHash);
            Role = role;
        }

        #region Metodos publicos
        public void UpdateProfile(string name, DateTimeOffset now)
        {
            SetName(name);
            MarkAsUpdated(now);
        }

        public void Delete(DateTimeOffset now)
        {
            if (IsDeleted) return;

            MarkAsDeleted(now);
        }

        public void ChangePassword(string newPasswordHash, DateTimeOffset now)
        {
            SetPassword(newPasswordHash);
            MarkAsUpdated(now);
        }

        public void ChangeRole(UserRole role, DateTimeOffset now)
        {
            if (Role == role)
                return;

            Role = role;
            MarkAsUpdated(now);
        }
        #endregion

        #region Metodos privados
        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleException("Nome é obrigatório.");

            if (name.Length > 150)
                throw new BusinessRuleException("Nome deve ter no máximo 150 caracteres.");

            Name = name.Trim();
        }

        private void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new BusinessRuleException("Email é obrigatório.");

            if (email.Length > 200)
                throw new BusinessRuleException("Email deve ter no máximo 200 caracteres.");

            Email = email.Trim().ToLowerInvariant();
        }

        private void SetPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new BusinessRuleException("Senha inválida.");
            PasswordHash = passwordHash;
        }
        #endregion
    }
}
