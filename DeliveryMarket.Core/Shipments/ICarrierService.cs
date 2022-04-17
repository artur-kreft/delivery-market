using System;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using FluentResults;

namespace DeliveryMarket.Core.Shipments
{
    public interface ICarrierService
    {
        Task<Result<ShipmentOffer>> MakeOfferAsync(Carrier carrier, Guid shipmentRequestId, decimal budget);
        Task<Result> BookShipmentAsync(Carrier carrier, Guid shipmentRequestId);

    }
}