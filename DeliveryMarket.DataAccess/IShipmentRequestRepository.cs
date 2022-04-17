using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model;
using DeliveryMarket.Core.Model.Shipment;
using FluentResults;

namespace DeliveryMarket.DataAccess
{
    // Assumption: at the moment we need only filter by active status
    // Todo: more advanced filtering by address, budget range, etc.

    public interface IShipmentRequestRepository
    {
        Task<Result<ShipmentRequest>> GetAsync(Guid id);
        Task<Result<List<ShipmentRequest>>> GetAsync(List<Guid> ids);
        Task<Result<List<ShipmentRequest>>> GetShipperRequestsAsync(Guid shipperId);
        Task<Result<List<ShipmentRequest>>> GetActiveShipperRequestsAsync(Guid shipperId);
        Task<Result<List<ShipmentRequest>>> GetAllIssuedAsync();
        Task<Result<ShipmentRequest>> CreateAsync(Guid shipperId, Route route, decimal budget, DateTime expireUtc, string notes = null);
        Task<Result<ShipmentRequest>> SetStatusAsync(Guid id, int version, ShipmentRequestStatusEnum newStatus);
    }
}