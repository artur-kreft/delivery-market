using System;
using System.Collections.Generic;

namespace DeliveryMarket.Core.Model.Shipment
{
    // Assumption: we are operating in USD
    // Todo: defining amount in many currencies?
    // Todo: service for currency convertion based on avg fx rates?

    public record ShipmentRequest
    {
        public readonly Guid Id;
        public readonly Guid ShipperId;
        public readonly Route Route;
        public readonly decimal Budget;
        public readonly string Notes;
        public readonly List<ShipmentOffer> Offers;
        public readonly ShipmentRequestStatusEnum Status;
        public readonly DateTime ExpireUtc;

        // Assumption: for simplicity versioning for update status is used
        // Todo: generate new status entity for each change to track event history of changes
        public int Version { get; }

        public ShipmentRequest(Guid shipperId, Route route, decimal budget, DateTime expireUtc, int version, string notes = null)
        {
            Version = version;
            Id = Guid.NewGuid();
            ShipperId = shipperId;
            Route = route;
            Budget = budget;
            ExpireUtc = expireUtc;
            Notes = notes;
            Offers = new List<ShipmentOffer>();
            Status = ShipmentRequestStatusEnum.Issued;
        }

        public ShipmentRequest(Guid shipperId, Route route, decimal budget, DateTime expireUtc, int version, ShipmentRequestStatusEnum status, string notes = null) :
            this(shipperId, route, budget, expireUtc, version, notes)
        {
            Status = status;
        }
    }
}