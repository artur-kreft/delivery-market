using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using DeliveryMarket.DataAccess;
using FluentResults;

namespace DeliveryMarket.Core.Shipments
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRequestRepository _requestRepository;

        public ShipmentService(IShipmentRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        /// <summary>
        /// Get shipment request by id
        /// </summary>
        /// <param name="id">shipment request id</param>
        /// <returns>shipment request</returns>
        public Task<Result<ShipmentRequest>> GetRequestAsync(Guid id)
        {
            return _requestRepository.GetAsync(id);
        }

        /// <summary>
        /// Get all shipment requests created by Shipper
        /// </summary>
        /// <param name="shipper">shipper</param>
        /// <returns>shipment requests</returns>
        public Task<Result<List<ShipmentRequest>>> GetShipperRequestsAsync(Shipper shipper)
        {
            return _requestRepository.GetShipperRequestsAsync(shipper.Id);
        }

        /// <summary>
        /// Get active only shipment requests created by Shipper
        /// </summary>
        /// <param name="shipper">shipper</param>
        /// <returns>shipment requests</returns>
        public Task<Result<List<ShipmentRequest>>> GetActiveShipperRequestsAsync(Shipper shipper)
        {
            return _requestRepository.GetActiveShipperRequestsAsync(shipper.Id);
        }

        /// <summary>
        /// Get all active shipment requests
        /// </summary>
        /// <returns>shipment requests</returns>
        public Task<Result<List<ShipmentRequest>>> GetAllActiveRequestsAsync()
        {
            return _requestRepository.GetAllIssuedAsync();
        }
    }
}