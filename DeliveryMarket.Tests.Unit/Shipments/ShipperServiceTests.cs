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
    public class ShipperServiceTests
    {
        private readonly Mock<IShipmentRequestRepository> _requestRepository;
        private readonly Mock<IShipmentOfferRepository> _offerRepository;
        private readonly Mock<IShipmentRepository> _shipmentRepository;
        private readonly IShipperService _shipperService;
        private readonly Shipper _shipper;

        public ShipperServiceTests()
        {
            _requestRepository = new Mock<IShipmentRequestRepository>();
            _offerRepository = new Mock<IShipmentOfferRepository>();
            _shipmentRepository = new Mock<IShipmentRepository>();
            _shipperService = new ShipperService(_requestRepository.Object, _offerRepository.Object, _shipmentRepository.Object);
            _shipper = new Shipper(Guid.NewGuid(), "Johny Bravo", "johny@bravo.pl");
        }

        [Fact]
        public async Task CreateRequest_should_fail_when_route_is_not_valid()
        {
            var route = new Route(new RoutePoint("", "", "", ""), new RoutePoint("", "", "", ""));
            var expire = new DateTime(2020, 3, 5, 12, 56, 9);
            var result = await _shipperService.CreateRequestAsync(_shipper, route, 43.65M, expire);
            Assert.True(result.IsFailed);

            route = new Route(new RoutePoint("a", "b", "c", "d"), new RoutePoint("e", "f", "g", ""));
            result = await _shipperService.CreateRequestAsync(_shipper, route, 43.65M, expire);
            Assert.True(result.IsFailed);

            route = new Route(new RoutePoint("a", "b", "c", "d"), null);
            result = await _shipperService.CreateRequestAsync(_shipper, route, 43.65M, expire);
            Assert.True(result.IsFailed);

            route = new Route(null, new RoutePoint("e", "f", "g", "z"));
            result = await _shipperService.CreateRequestAsync(_shipper, route, 43.65M, expire);
            Assert.True(result.IsFailed);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.001)]
        [InlineData(-198.98)]
        public async Task CreateRequest_should_fail_when_budget_is_not_positive(decimal budget)
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var expire = new DateTime(2020, 3, 5, 12, 56, 9);
            var result = await _shipperService.CreateRequestAsync(_shipper, route, budget, expire);
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task CreateRequest_should_fail_when_expiration_is_before_today()
        {
            var expireDate = new DateTime(2019, 8, 7, 15, 6, 7);
            DateTime.SpecifyKind(expireDate, DateTimeKind.Utc);

            var today = new DateTime(2019, 8, 7, 15, 6, 8);
            DateTime.SpecifyKind(expireDate, DateTimeKind.Utc);

            using var o = new OverrideDateTimeProvider(new DateTimeOffset(today, TimeSpan.Zero));

            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var result = await _shipperService.CreateRequestAsync(_shipper, route, 543, expireDate);
            Assert.True(result.IsFailed);
        }

        [Theory]
        [InlineData(Booked)]
        [InlineData(ShipmentRequestStatusEnum.Canceled)]
        [InlineData(Expired)]
        public async Task AcceptOffer_should_fail_when_request_is_not_issued(ShipmentRequestStatusEnum status)
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0, status);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            var result = await _shipperService.AcceptOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task AcceptOffer_should_fail_when_request_is_null()
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(default(ShipmentRequest))));

            var result = await _shipperService.AcceptOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task AcceptOffer_should_fail_when_shipper_id_does_not_match()
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            var result = await _shipperService.AcceptOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Theory]
        [InlineData(ShipmentOfferStatusEnum.Accepted)]
        [InlineData(ShipmentOfferStatusEnum.Canceled)]
        [InlineData(ShipmentOfferStatusEnum.Rejected)]
        [InlineData(ShipmentOfferStatusEnum.Replaced)]
        public async Task AcceptOffer_should_fail_when_offer_is_not_issued(ShipmentOfferStatusEnum status)
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(_shipper.Id, route, 123, DateTime.MaxValue, 0);
            var offer = new ShipmentOffer(Guid.NewGuid(), Guid.NewGuid(), request.Id, 23, 0, status);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _offerRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            var result = await _shipperService.AcceptOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task AcceptOffer_should_update_statuses_when_succeeded()
        {
            var expireDate = new DateTime(2019, 8, 7, 15, 6, 7);
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(_shipper.Id, route, 123, DateTime.MaxValue, 0);
            var offer = new ShipmentOffer(Guid.NewGuid(), Guid.NewGuid(), request.Id, 23, 0);
            var shipment = new Shipment(Guid.NewGuid(), request.Id, offer.Id, expireDate);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _requestRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentRequestStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _offerRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _offerRepository
                .Setup(it => it.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _offerRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentOfferStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _shipmentRepository
                .Setup(it => it.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(Result.Ok(shipment)));

            var result = await _shipperService.AcceptOfferAsync(_shipper, request.Id, offer.Id);
            Assert.True(result.IsSuccess);
            
            _requestRepository.Verify(it => it.SetStatusAsync(request.Id, request.Version, Booked), Times.Once);
            _offerRepository.Verify(it => it.SetStatusAsync(offer.Id, offer.Version, Accepted), Times.Once);
            _shipmentRepository.Verify(it => it.CreateAsync(request.Id, offer.Id, It.IsAny<DateTime>()), Times.Once);
        }

        [Theory]
        [InlineData(Booked)]
        [InlineData(ShipmentRequestStatusEnum.Canceled)]
        [InlineData(Expired)]
        public async Task RejectOffer_should_fail_when_request_is_not_issued(ShipmentRequestStatusEnum status)
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0, status);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            var result = await _shipperService.RejectOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task RejectOffer_should_fail_when_request_is_null()
        {
            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(default(ShipmentRequest))));

            var result = await _shipperService.RejectOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task RejectOffer_should_fail_when_shipper_id_does_not_match()
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(Guid.NewGuid(), route, 123, DateTime.MaxValue, 0);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            var result = await _shipperService.RejectOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Theory]
        [InlineData(ShipmentOfferStatusEnum.Accepted)]
        [InlineData(ShipmentOfferStatusEnum.Canceled)]
        [InlineData(ShipmentOfferStatusEnum.Rejected)]
        [InlineData(ShipmentOfferStatusEnum.Replaced)]
        public async Task RejectOffer_should_fail_when_offer_is_not_issued(ShipmentOfferStatusEnum status)
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(_shipper.Id, route, 123, DateTime.MaxValue, 0);
            var offer = new ShipmentOffer(Guid.NewGuid(), Guid.NewGuid(), request.Id, 23, 0, status);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _offerRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            var result = await _shipperService.RejectOfferAsync(_shipper, Guid.NewGuid(), Guid.NewGuid());
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task RejectOffer_should_update_statuses_when_succeeded()
        {
            var route = new Route(new RoutePoint("a", "a", "a", "a"), new RoutePoint("a", "a", "a", "a"));
            var request = new ShipmentRequest(_shipper.Id, route, 123, DateTime.MaxValue, 0);
            var offer = new ShipmentOffer(Guid.NewGuid(), Guid.NewGuid(), request.Id, 23, 0);

            _requestRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _requestRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentRequestStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(request)));

            _offerRepository
                .Setup(it => it.GetAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _offerRepository
                .Setup(it => it.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            _offerRepository
                .Setup(it => it.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<ShipmentOfferStatusEnum>()))
                .Returns(Task.FromResult(Result.Ok(offer)));

            var result = await _shipperService.RejectOfferAsync(_shipper, request.Id, offer.Id);
            Assert.True(result.IsSuccess);
            
            _offerRepository.Verify(it => it.SetStatusAsync(offer.Id, offer.Version, Rejected), Times.Once);
        }
    }
}