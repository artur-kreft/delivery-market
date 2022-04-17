using System;

namespace DeliveryMarket.Core.Model.Shipment
{
    public record Route
    {
        public readonly RoutePoint Pickup;
        public readonly RoutePoint Destination;
        public bool IsValid => Pickup != null && Pickup.IsValid && Destination != null && Destination.IsValid;

        public Route(RoutePoint pickup, RoutePoint destination)
        {
            Pickup = pickup;
            Destination = destination;
        }
    }
}