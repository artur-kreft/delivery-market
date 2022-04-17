using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using DeliveryMarket.Core.Shipments;
using DeliveryMarket.DataAccess;
using DeliveryMarket.Notification;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static DeliveryMarket.Core.Model.Shipment.ShipmentOfferStatusEnum;
using static DeliveryMarket.Core.Model.Shipment.ShipmentRequestStatusEnum;
using static DeliveryMarket.Core.Model.Shipment.ShipmentStatusEnum;
using Range = System.Range;

namespace DeliveryMarket.Tests.Unit.Shipments
{
    public class DataCleanerServiceTests
    {
        private readonly Mock<IShipmentRequestRepository> _requestRepository;
        private readonly Mock<IShipmentOfferRepository> _offerRepository;
        private readonly Mock<IShipmentRepository> _shipmentRepository;
        private readonly Mock<INotifyService> _notifyService;
        private readonly Mock<ILogger<IDataCleanerService>> _logger;
        private readonly IDataCleanerService _dataCleanerService;
        private readonly Carrier _carrier;
        private readonly Shipper _shipper;

        public DataCleanerServiceTests()
        {
            _requestRepository = new Mock<IShipmentRequestRepository>();
            _offerRepository = new Mock<IShipmentOfferRepository>();
            _shipmentRepository = new Mock<IShipmentRepository>();
            _notifyService = new Mock<INotifyService>();
            _logger = new Mock<ILogger<IDataCleanerService>>();
            _dataCleanerService = new DataCleanerService(_logger.Object, _requestRepository.Object, _offerRepository.Object, _shipmentRepository.Object, _notifyService.Object);
            _carrier = new Carrier(Guid.NewGuid(), "Johny Bravo", "johny@bravo.pl");
            _shipper = new Shipper(Guid.NewGuid(), "Michael Jordan", "michael@jordan.pl");
        }

        [Fact]
        public async Task ConfirmShipments_should_do_nothing_when_no_submitted_shipments()
        {
            _shipmentRepository
                .Setup(it => it.GetAllSubmittedShipmentsAsync())
                .Returns(Task.FromResult(Result.Ok(new List<Shipment>())));

            await _dataCleanerService.ConfirmShipmentsAsync();
            _logger.VerifyLogging("No submitted shipments found", LogLevel.Information, Times.Once());
        }

        [Fact]
        public async Task ConfirmShipments_should_reject_and_accept_and_notify()
        {
            var route = new Route(new RoutePoint("", "", "", ""), new RoutePoint("", "", "", ""));

            var requests = new List<ShipmentRequest>
            {
                new (_shipper.Id, route, 123, DateTime.MaxValue, 0),
            };

            var offers = new List<ShipmentOffer>
            {
                new (Guid.NewGuid(), _carrier.Id, requests[0].Id, 65.98M, 0),
                new (Guid.NewGuid(), _carrier.Id, requests[0].Id, 35.98M, 0),
                new (Guid.NewGuid(), _carrier.Id, requests[0].Id, 765.98M, 0),
                new (Guid.NewGuid(), _carrier.Id, requests[0].Id, 95.98M, 0),
            };

            foreach (var request in requests)
            {
                request.Offers.AddRange(offers);
            }

            var shipments = new List<Shipment>
            {
                new (Guid.NewGuid(), requests[0].Id, offers[0].Id, new DateTime(2019, 4, 2)),
                new (Guid.NewGuid(), requests[0].Id, offers[1].Id, new DateTime(2019, 4, 5)),
                new (Guid.NewGuid(), requests[0].Id, offers[2].Id, new DateTime(2019, 4, 1)),
                new (Guid.NewGuid(), requests[0].Id, offers[3].Id, new DateTime(2019, 4, 3)),
            };

            _shipmentRepository
                .Setup(it => it.GetAllSubmittedShipmentsAsync())
                .Returns(Task.FromResult(Result.Ok(shipments)));

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<List<Guid>>()))
                .Returns(Task.FromResult(Result.Ok(requests)));

            _requestRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentRequestStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(requests[0])));

            _offerRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentOfferStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(offers[0])));

            _shipmentRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<ShipmentStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(shipments[0])));

            await _dataCleanerService.ConfirmShipmentsAsync();

            _offerRepository.Verify(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), Rejected), Times.Exactly(3));
            _offerRepository.Verify(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), Accepted), Times.Once);
            _requestRepository.Verify(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), Booked), Times.Once);
            _shipmentRepository.Verify(it => it.SetStatusAsync(It.IsAny<Guid>(), Reverted), Times.Exactly(3));
            _shipmentRepository.Verify(it => it.SetStatusAsync(It.IsAny<Guid>(), Confirmed), Times.Once);

            _notifyService.Verify(it => it.Notify(_carrier.Id, "Offer rejected", It.IsAny<string>()), Times.Exactly(3));
            _notifyService.Verify(it => it.Notify(_carrier.Id, "Shipment confirmed", It.IsAny<string>()), Times.Once);
            _notifyService.Verify(it => it.Notify(_shipper.Id, "Shipment confirmed", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmShipments_should_abort_when_request_is_missing()
        {
            var id = Guid.NewGuid();
            var shipments = new List<Shipment>
            {
                new (Guid.NewGuid(), id, Guid.NewGuid(), new DateTime(2019, 4, 2)),
                new (Guid.NewGuid(), id, Guid.NewGuid(), new DateTime(2019, 4, 5))
            };

            _shipmentRepository
                .Setup(it => it.GetAllSubmittedShipmentsAsync())
                .Returns(Task.FromResult(Result.Ok(shipments)));

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<List<Guid>>()))
                .Returns(Task.FromResult(Result.Ok(new List<ShipmentRequest>())));

            _shipmentRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<ShipmentStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(shipments[0])));

            await _dataCleanerService.ConfirmShipmentsAsync();

            _logger.VerifyLogging("Request was not found", LogLevel.Warning, Times.Once());
            _shipmentRepository.Verify(it => it.SetStatusAsync(It.IsAny<Guid>(), Aborted), Times.Exactly(2));
        }
    }
}