using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model;
using DeliveryMarket.Core.Model.Shipment;
using FluentResults;

namespace DeliveryMarket.DataAccess
{
    public interface IShipmentRepository
    {
        Task<Result<Shipment>> GetAsync(Guid id);
        Task<Result<Shipment>> GetConfirmedRequestShipmentAsync(Guid shipmentRequestId);
        Task<Result<List<Shipment>>> GetAllRequestShipmentsAsync(Guid shipmentRequestId);
        Task<Result<List<Shipment>>> GetAllSubmittedShipmentsAsync();
        Task<Result<Shipment>> CreateAsync(Guid shipmentRequestId, Guid shipmentOfferId, DateTime booked);
        Task<Result<Shipment>> SetStatusAsync(Guid id, ShipmentStatusEnum newStatus);
    }
}