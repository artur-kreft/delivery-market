using System;

namespace DeliveryMarket.Core.Model.User
{
    public record Shipper : User
    {
        public override UserTypeEnum Type => UserTypeEnum.Shipper;

        public Shipper(Guid id, string name, string email) : base(id, name, email)
        {
        }
    }
}