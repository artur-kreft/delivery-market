using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.DataAccess;
using DeliveryMarket.Notification;
using Microsoft.Extensions.Logging;
using static DeliveryMarket.Core.Model.Shipment.ShipmentOfferStatusEnum;
using static DeliveryMarket.Core.Model.Shipment.ShipmentRequestStatusEnum;
using static DeliveryMarket.Core.Model.Shipment.ShipmentStatusEnum;

namespace DeliveryMarket.Core.Shipments
{
    public class DataCleanerService : IDataCleanerService
    {
        private readonly ILogger<IDataCleanerService> _logger;
        private readonly IShipmentRequestRepository _requestRepository;
        private readonly IShipmentOfferRepository _offerRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly INotifyService _notifyService;

        public DataCleanerService
        (
            ILogger<IDataCleanerService> logger,
            IShipmentRequestRepository requestRepository,
            IShipmentOfferRepository offerRepository,
            IShipmentRepository shipmentRepository,
            INotifyService notifyService
        )
        {
            _logger = logger;
            _requestRepository = requestRepository;
            _offerRepository = offerRepository;
            _shipmentRepository = shipmentRepository;
            _notifyService = notifyService;
        }

        /// <summary>
        /// Go through all submitted shipments, confirm or
        /// revert and send proper notifications 
        /// </summary>
        /// <returns></returns>
        public async Task ConfirmShipmentsAsync()
        {
            var shipmentsResult = await _shipmentRepository.GetAllSubmittedShipmentsAsync();
            if (shipmentsResult.IsFailed)
            {
                _logger.LogError(string.Join("; ", shipmentsResult.Errors));
                return;
            }

            if (!shipmentsResult.Value.Any())
            {
                _logger.LogInformation("No submitted shipments found");
                return;
            }

            var requestIds = shipmentsResult.Value.Select(it => it.ShipmentRequestId).Distinct().ToList();
            var requestsResult = await _requestRepository.GetAsync(requestIds);
            if (requestsResult.IsFailed)
            {
                _logger.LogError(string.Join("; ", requestsResult.Errors));
                return;
            }
            
            var requests = requestsResult.Value.ToDictionary(it => it.Id);

            shipmentsResult.Value
                .GroupBy(it => it.ShipmentRequestId)
                .AsParallel()
                .ForAll((async grouping =>
                {
                    var shipments = grouping.ToList();

                    if (requests.ContainsKey(grouping.Key) == false)
                    {
                        _logger.LogWarning("Request was not found: {0}", grouping.Key);
                        shipments.ForEach(it => _shipmentRepository.SetStatusAsync(it.Id, Aborted));
                        return;
                    }

                    var request = requests[grouping.Key];
                    var first = shipments.OrderBy(it => it.Booked).First();

                    foreach (var shipment in shipments)
                    {
                        var offer = request.Offers.FirstOrDefault(it => it.Id == shipment.ShipmentOfferId);
                        if (offer == null)
                        {
                            _logger.LogError("No matching offer found for shipment: {0}", shipment.Id);
                            continue;
                        }

                        if (first == shipment)
                        {
                            var resultConfirm = await _shipmentRepository.SetStatusAsync(shipment.Id, Confirmed);
                            if (resultConfirm.IsFailed)
                            {
                                _logger.LogError("Failed to confirm shipment: {0}", shipment.Id);
                                continue;
                            }

                            await Book(request, offer);
                            continue;
                        }

                        var resultRevert = await _shipmentRepository.SetStatusAsync(shipment.Id, Reverted);
                        if (resultRevert.IsFailed)
                        {
                            _logger.LogError("Failed to confirm shipment: {0}", shipment.Id);
                        }

                        await Reject(offer);
                    }

                }));

        }

        private async Task Book(ShipmentRequest request, ShipmentOffer offer)
        {
            var requestResult = await _requestRepository.SetStatusAsync(request.Id, request.Version, Booked);
            if (requestResult.IsFailed)
            {
                _logger.LogError("Failed to book request: {0}", request.Id);
            }

            var resultOffer = await _offerRepository.SetStatusAsync(offer.Id, offer.Version, Accepted);
            if (resultOffer.IsFailed)
            {
                _logger.LogError("Failed to confirm offer: {0}", offer.Id);
            }

            _notifyService.Notify(request.ShipperId, "Shipment confirmed",
                "Congrats! You will get your product delivered!");
            _notifyService.Notify(offer.CarrierId, "Shipment confirmed",
                "Congrats! You have a job to do!");
        }

        private async Task Reject(ShipmentOffer offer)
        {
            var resultOffer = await _offerRepository.SetStatusAsync(offer.Id, offer.Version, Rejected);
            if (resultOffer.IsFailed)
            {
                _logger.LogError("Failed to reject offer: {0}", offer.Id);
            }
            
            _notifyService.Notify(offer.CarrierId, "Offer rejected",
                "Unfortunately your offer was rejected :(");
        }
    }
}