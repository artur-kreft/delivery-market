using System;

namespace DeliveryMarket.Core.Model.User
{
    public record Carrier : User
    {
        public override UserTypeEnum Type => UserTypeEnum.Carrier;

        public Carrier(Guid id, string name, string email) : base(id, name, email)
        {
        }
    }
}