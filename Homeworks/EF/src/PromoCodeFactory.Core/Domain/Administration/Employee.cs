using System;

namespace PromoCodeFactory.Core.Domain.Administration
{
    public class Employee
        : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        // Можно так, без идентификатора
        public virtual Role Role { get; set; }
        // А можно с ID:
        //public Guid RoleId { get; set; }
        //public virtual Role Role { get; set; }

        public int AppliedPromocodesCount { get; set; }
    }
}