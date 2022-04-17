using System;

namespace DeliveryMarket.Core.Model.Shipment
{
    public record Shipment
    {
        public readonly Guid Id;
        public readonly Guid ShipmentRequestId;
        public readonly Guid ShipmentOfferId;
        public readonly ShipmentStatusEnum Status;
        public readonly DateTime Booked;

        public Shipment(Guid id, Guid shipmentRequestId, Guid shipmentOfferId, DateTime booked)
        {
            Id = id;
            ShipmentRequestId = shipmentRequestId;
            ShipmentOfferId = shipmentOfferId;
            Status = ShipmentStatusEnum.Submitted;
            Booked = booked;
        }

        public Shipment(Guid id, Guid shipmentRequestId, Guid shipmentOfferId, DateTime booked, ShipmentStatusEnum status)
        :this(id, shipmentRequestId, shipmentOfferId, booked)
        {
            Status = status;
        }
    }
}