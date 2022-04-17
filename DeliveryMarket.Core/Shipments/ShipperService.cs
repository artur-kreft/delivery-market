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
    public class ShipperService : IShipperService
    {
        private readonly IShipmentRequestRepository _requestRepository;
        private readonly IShipmentOfferRepository _offerRepository;
        private readonly IShipmentRepository _shipmentRepository;

        public ShipperService
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
        /// Create new shipment request by Shipper
        /// </summary>
        /// <param name="shipper">shipper</param>
        /// <param name="route">route of shipment</param>
        /// <param name="budget">price that Shipper is willing to pay</param>
        /// <param name="expireUtc">expiration time of this request</param>
        /// <param name="notes">additional information</param>
        /// <returns>shipment request</returns>
        public Task<Result<ShipmentRequest>> CreateRequestAsync(Shipper shipper, Route route, decimal budget, DateTime expireUtc, string notes = null)
        {
            if (route == null || !route.IsValid)
            {
                Result.Fail("Full route is not defined");
            }

            if (budget <= 0)
            {
                Result.Fail("Budget has to larger than 0");
            }

            if (expireUtc <= DateTimeProvider.UtcNow)
            {
                Result.Fail("Expire time has to be in the future");
            }

            return _requestRepository.CreateAsync(shipper.Id, route, budget, expireUtc, notes);
        }

        /// <summary>
        /// Shipper accepts offer
        /// </summary>
        /// <param name="shipper">shipper</param>
        /// <param name="shipmentRequestId">request id created by Shipper</param>
        /// <param name="shipmentOfferId">offer id to accept</param>
        /// <returns></returns>
        public async Task<Result> AcceptOfferAsync(Shipper shipper, Guid shipmentRequestId, Guid shipmentOfferId)
        {
            var requestResult = await GetIssuedRequest(shipper, shipmentRequestId);
            if (requestResult.IsFailed)
            {
                return Result.Fail(requestResult.Errors[0].Message);
            }

            var offersResult = await GetIssuedOffers(shipmentOfferId);
            if (offersResult.IsFailed)
            {
                return Result.Fail(offersResult.Errors[0].Message);
            }

            var shipmentResult =
                await _shipmentRepository.CreateAsync(shipmentRequestId, shipmentOfferId, DateTime.UtcNow);

            if (shipmentResult.IsFailed)
            {
                return Result.Fail(shipmentResult.Errors[0].Message);
            }

            var requestStatusResult =
                await _requestRepository.SetStatusAsync(shipmentRequestId, requestResult.Value.Version, Booked);
            var offerStatusResult =
                await _offerRepository.SetStatusAsync(shipmentOfferId, offersResult.Value.Version, Accepted);

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

        /// <summary>
        /// Shipper rejects offer
        /// </summary>
        /// <param name="shipper">shipper</param>
        /// <param name="shipmentRequestId">request id created by Shipper</param>
        /// <param name="shipmentOfferId">offer id to reject</param>
        /// <returns></returns>
        public async Task<Result<ShipmentRequest>> RejectOfferAsync(Shipper shipper, Guid shipmentRequestId, Guid shipmentOfferId)
        {
            var requestResult = await GetIssuedRequest(shipper, shipmentRequestId);
            if (requestResult.IsFailed)
            {
                return Result.Fail(requestResult.Errors[0].Message);
            }

            var offersResult = await GetIssuedOffers(shipmentOfferId);
            if (offersResult.IsFailed)
            {
                return Result.Fail(offersResult.Errors[0].Message);
            }
            var offerStatusResult =
                await _offerRepository.SetStatusAsync(shipmentOfferId, offersResult.Value.Version, Rejected);

            if (offerStatusResult.IsFailed)
            {
                return Result.Fail(offerStatusResult.Errors[0].Message);
            }

            return Result.Ok();
        }

        private async Task<Result<ShipmentRequest>> GetIssuedRequest(Shipper shipper, Guid shipmentRequestId)
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

            if (requestResult.Value.ShipperId != shipper.Id)
            {
                return Result.Fail($"{shipper.Id} is not owner of this request");
            }

            return requestResult;
        }

        private async Task<Result<ShipmentOffer>> GetIssuedOffers(Guid shipmentOfferId)
        {
            var offersResult = await _offerRepository.GetAsync(shipmentOfferId);
            if (offersResult.IsFailed)
            {
                return Result.Fail("Failed to fetch offer");
            }

            if (offersResult.Value == null)
            {
                return Result.Fail("Offer does not exist");
            }

            if (offersResult.Value.Status != ShipmentOfferStatusEnum.Issued)
            {
                return Result.Fail($"Request is {offersResult.Value.Status}");
            }

            return offersResult;
        }
    }
}