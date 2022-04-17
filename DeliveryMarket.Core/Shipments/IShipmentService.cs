using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using FluentResults;

namespace DeliveryMarket.Core.Shipments
{
    public interface IShipmentService
    {
        Task<Result<ShipmentRequest>> GetRequestAsync(Guid id);
        Task<Result<List<ShipmentRequest>>> GetShipperRequestsAsync(Shipper shipper);
        Task<Result<List<ShipmentRequest>>> GetActiveShipperRequestsAsync(Shipper shipper);
        Task<Result<List<ShipmentRequest>>> GetAllActiveRequestsAsync();
    }
}