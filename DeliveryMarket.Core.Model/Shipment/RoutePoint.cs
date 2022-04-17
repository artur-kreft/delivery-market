using System;

namespace DeliveryMarket.Core.Model.Shipment
{
    // Assumption: we are operating in USA
    // Todo: address formats and more advanced validation for many countries

    public record RoutePoint
    {
        public readonly string City;
        public readonly string State;
        public readonly string ZipCode;
        public readonly string Address;

        public bool IsValid =>
            false == (string.IsNullOrEmpty(City)
                      || string.IsNullOrEmpty(State)
                      || string.IsNullOrEmpty(ZipCode)
                      || string.IsNullOrEmpty(Address));

        public RoutePoint(string city, string state, string zipCode, string address)
        {
            City = city;
            State = state;
            ZipCode = zipCode;
            Address = address;
        }
    }
}