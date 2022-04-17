using System;

namespace DeliveryMarket.Core.Model.User
{
    public abstract record User
    {
        public readonly Guid Id;
        public readonly string Name;
        public readonly string Email;
        public virtual UserTypeEnum Type { get; }

        protected User(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }
}