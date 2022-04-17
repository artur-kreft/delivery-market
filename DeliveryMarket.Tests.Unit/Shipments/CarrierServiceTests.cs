using System;
using System.Threading.Tasks;
using DateTimeProviders;
using DeliveryMarket.Core.Model.Shipment;
using DeliveryMarket.Core.Model.User;
using DeliveryMarket.Core.Shipments;
using DeliveryMarket.DataAccess;
using FluentResults;
using Moq;
using Xunit;
using static DeliveryMarket.Core.Model.Shipment.ShipmentOfferStatusEnum;
using static DeliveryMarket.Core.Model.Shipment.ShipmentRequestStatusEnum;

namespace DeliveryMarket.Tests.Unit.Shipments
{
    public class CarrierServiceTests
    {
        private readonly Mock<IShipmentRequestRepository> _requestRepository;
        private readonly Mock<IShipmentOfferRepository> _offerRepository;
        private readonly Mock<IShipmentRepository> _shipmentRepository;
        private readonly ICarrierService _carrierService;
        private readonly Carrier _carrier;

        public CarrierServiceTests()
        {
            _requestRepository = new Mock<IShipmentRequestRepository>();
            _offerRepository = new Mock<IShipmentOfferRepository>();
            _shipmentRepository = new Mock<IShipmentRepository>();
            _carrierService = new CarrierService(_requestRepository.Object, _offerRepository.Object, _shipmentRepository.Object);
            _carrier = new Carrier(Guid.NewGuid(), "Johny Bravo", "johny@bravo.pl");
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-0.001)]
        [InlineData(-198.98)]
        public async Task MakeOffer_should_fail_when_budget_is_not_positive(decimal budget)
        {
            var result = await _carrierService.MakeOfferAsync(_carrier, Guid.NewGuid(), budget);
            Assert.True(result.IsFailed);
        }

        [Theory]
        [InlineData(Booked)]
        [InlineData(ShipmentRequestStatusEnum.Canceled)]
        [InlineData(Expired)]
        public async Task MakeOffer_should_fail_when_request_is_not_issued(ShipmentRequestStatusEnum status)
        {
            var route = new Route(new RoutePoint("", "", "", ""), new RoutePoint("", "", "", ""));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0, status);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            var result = await _carrierService.MakeOfferAsync(_carrier, Guid.NewGuid(), 123.00M);
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task MakeOffer_should_fail_when_request_is_null()
        {
            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(default(ShipmentRequest))));

            var result = await _carrierService.MakeOfferAsync(_carrier, Guid.NewGuid(), 123.00M);
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task MakeOffer_should_call_repo()
        {
            var route = new Route(new RoutePoint("", "", "", ""), new RoutePoint("", "", "", ""));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0, ShipmentRequestStatusEnum.Issued);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _offerRepository
                .Setup(it => it.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(Result.Ok(default(ShipmentOffer))));

            var budget = 438.98M;
            var id = Guid.NewGuid();
            var result = await _carrierService.MakeOfferAsync(_carrier, id, budget);
            Assert.True(result.IsSuccess);

            _offerRepository.Verify(it => it.CreateAsync(_carrier.Id, id, budget), Times.Once);
        }

        [Theory]
        [InlineData(Booked)]
        [InlineData(ShipmentRequestStatusEnum.Canceled)]
        [InlineData(Expired)]
        public async Task BookShipment_should_fail_when_request_is_not_issued(ShipmentRequestStatusEnum status)
        {
            var route = new Route(new RoutePoint("", "", "", ""), new RoutePoint("", "", "", ""));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0, status);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            var result = await _carrierService.BookShipmentAsync(_carrier, Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task BookShipment_should_fail_when_request_is_null()
        {
            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(default(ShipmentRequest))));

            var result = await _carrierService.BookShipmentAsync(_carrier, Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task BookShipment_should_call_offer_repo()
        {
            var budget = 438.98M;
            var route = new Route(new RoutePoint("", "", "", ""), new RoutePoint("", "", "", ""));

            var request = new ShipmentRequest(Guid.NewGuid(), route, budget, DateTime.MaxValue, 1, ShipmentRequestStatusEnum.Issued);
            var offerId = Guid.NewGuid();
            var offer = new ShipmentOffer(offerId, _carrier.Id, request.Id, budget, 1);
            var date = new DateTime(2019, 8, 7, 15, 6, 7);
            DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var shipment = new Core.Model.Shipment.Shipment(Guid.NewGuid(), request.Id, offerId, date);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _requestRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentRequestStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _offerRepository
                .Setup(it => it.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _offerRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentOfferStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _shipmentRepository
                .Setup(it => it.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(Result.Ok(shipment)));


            using var o = new OverrideDateTimeProvider(new DateTimeOffset(date, TimeSpan.Zero));

            var result = await _carrierService.BookShipmentAsync(_carrier, request.Id);
            Assert.True(result.IsSuccess);
            _requestRepository.Verify(it => it.GetAsync(request.Id), Times.Once);
            _requestRepository.Verify(it => it.SetStatusAsync(request.Id, request.Version, Booked), Times.Once);
            _offerRepository.Verify(it => it.CreateAsync(_carrier.Id, request.Id, budget), Times.Once);
            _offerRepository.Verify(it => it.SetStatusAsync(offerId, offer.Version, Accepted), Times.Once);
            _shipmentRepository.Verify(it => it.CreateAsync(request.Id, offerId, date), Times.Once);
        }
    }
}