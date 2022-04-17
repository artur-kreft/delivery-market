using System;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using DeliveryMarket.DataAccess;
using FluentResults;
using static DeliveryMarket.Core.Model.Shipment.ShipmentOfferStatusEnum;
using static DeliveryMarket.Core.Model.Shipment.ShipmentRequestStatusEnum;

namespace DeliveryMarket.Core.Shipments
{
    public class CarrierService : ICarrierService
    {
        private readonly IShipmentRequestRepository _requestRepository;
        private readonly IShipmentOfferRepository _offerRepository;
        private readonly IShipmentRepository _shipmentRepository;

        public CarrierService
        (
            IShipmentRequestRepository requestRepository,
            IShipmentOfferRepository offerRepository,
            IShipmentRepository shipmentRepository
        )
        {
            _requestRepository = requestRepository;
            _offerRepository = offerRepository;
            _shipmentRepository = shipmentRepository;
        }

        /// <summary>
        /// Create new offer for issued shipment request
        /// </summary>
        /// <param name="carrier">carrier user</param>
        /// <param name="shipmentRequestId">request id</param>
        /// <param name="budget">expected price for shipment</param>
        /// <returns>created offer</returns>
        public async Task<Result<ShipmentOffer>> MakeOfferAsync(Carrier carrier, Guid shipmentRequestId, decimal budget)
        {
            if (budget <= 0)
            {
                return Result.Fail("Budget has to larger than 0");
            }

            var requestResult = await GetIssuedRequest(shipmentRequestId);
            if (requestResult.IsFailed)
            {
                return Result.Fail(requestResult.Errors[0].Message);
            }

            var offerResult = await _offerRepository.CreateAsync(carrier.Id, shipmentRequestId, budget);
            if (offerResult.IsFailed)
            {
                return Result.Fail(offerResult.Errors[0].Message);
            }

            return offerResult;
        }

        /// <summary>
        /// Accept issued shipment request terms and book it
        /// </summary>
        /// <param name="carrier">carrier user</param>
        /// <param name="shipmentRequestId">request id</param>
        /// <returns></returns>
        public async Task<Result> BookShipmentAsync(Carrier carrier, Guid shipmentRequestId)
        {
            var requestResult = await GetIssuedRequest(shipmentRequestId);
            if (requestResult.IsFailed)
            {
                return Result.Fail(requestResult.Errors[0].Message);
            }

            var offerResult = await _offerRepository.CreateAsync(carrier.Id, shipmentRequestId, requestResult.Value.Budget);
            if (offerResult.IsFailed)
            {
                return Result.Fail(offerResult.Errors[0].Message);
            }

            var shipmentOfferId = offerResult.Value.Id;
            var shipmentResult =
                await _shipmentRepository.CreateAsync(shipmentRequestId, shipmentOfferId, DateTimeProvider.UtcNow);
            if (shipmentResult.IsFailed)
            {
                return Result.Fail(shipmentResult.Errors[0].Message);
            }

            var requestStatusResult =
                await _requestRepository.SetStatusAsync(shipmentRequestId, requestResult.Value.Version, Booked);
            var offerStatusResult =
                await _offerRepository.SetStatusAsync(shipmentOfferId, offerResult.Value.Version, Accepted);

            if (requestStatusResult.IsFailed)
            {
                return Result.Fail(requestStatusResult.Errors[0].Message);
            }

            if (offerStatusResult.IsFailed)
            {
                return Result.Fail(offerStatusResult.Errors[0].Message);
            }

            return Result.Ok();
        }

        private async Task<Result<ShipmentRequest>> GetIssuedRequest(Guid shipmentRequestId)
        {
            var requestResult = await _requestRepository.GetAsync(shipmentRequestId);
            if (requestResult.IsFailed)
            {
                return Result.Fail("Failed to fetch request");
            }

            if (requestResult.Value == null)
            {
                return Result.Fail("Request does not exist");
            }

            if (requestResult.Value.Status != ShipmentRequestStatusEnum.Issued)
            {
                return Result.Fail($"Request is {requestResult.Value.Status}");
            }

            return requestResult;
        }
    }
}