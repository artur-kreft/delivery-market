using System;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using FluentResults;

namespace DeliveryMarket.Core.Shipments
{
    public interface IShipperService
    {
        Task<Result<ShipmentRequest>> CreateRequestAsync(Shipper shipper, Route route, decimal budget, DateTime expireUtc, string notes = null);
        Task<Result> AcceptOfferAsync(Shipper shipper, Guid shipmentRequestId, Guid shipmentOfferId);
        Task<Result<ShipmentRequest>> RejectOfferAsync(Shipper shipper, Guid shipmentRequestId, Guid shipmentOfferId);
    }
}