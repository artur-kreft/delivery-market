using System;

namespace DeliveryMarket.Core.Model.Shipment
{
    public record ShipmentOffer
    {
        public readonly Guid Id;
        public readonly Guid CarrierId;
        public readonly Guid ShipmentRequestId;
        public readonly decimal Budget;
        public readonly ShipmentOfferStatusEnum Status;

        // Assumption: for simplicity versioning for update status is used
        // Todo: generate new status entity for each change to track event history of changes
        public int Version { get; } 

        public ShipmentOffer
        (
            Guid id, 
            Guid carrierId, 
            Guid shipmentRequestId, 
            decimal budget, 
            int version
        )
        {
            Id = id;
            CarrierId = carrierId;
            ShipmentRequestId = shipmentRequestId;
            Budget = budget;
            Version = version;
            Status = ShipmentOfferStatusEnum.Issued;
        }

        public ShipmentOffer
        (
            Guid id,
            Guid carrierId,
            Guid shipmentRequestId,
            decimal budget,
            int version,
            ShipmentOfferStatusEnum status
        ) : this(id, carrierId, shipmentRequestId, budget, version)
        {
            Status = status;
        }
    }
}