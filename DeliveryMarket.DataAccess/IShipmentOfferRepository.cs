using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model;
using DeliveryMarket.Core.Model.Shipment;
using FluentResults;

namespace DeliveryMarket.DataAccess
{
    public interface IShipmentOfferRepository
    {
        Task<Result<ShipmentOffer>> GetAsync(Guid id);
        Task<Result<List<ShipmentOffer>>> GetRequestOffersAsync(Guid shipmentRequestId);
        Task<Result<List<ShipmentOffer>>> GetRequestActiveOffersAsync(Guid shipmentRequestId);
        Task<Result<List<ShipmentOffer>>> GetCarrierActiveOffersAsync(Guid carrierId);
        Task<Result<ShipmentOffer>> CreateAsync(Guid carrierId, Guid shipmentRequestId, decimal budget);
        Task<Result<ShipmentOffer>> SetStatusAsync(Guid id, int version, ShipmentOfferStatusEnum newStatus);
    }
}